using AILearningApp.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;

namespace AILearningApp.Services;

/// <summary>
/// Service for managing user progress using browser local storage
/// </summary>
public interface IProgressService
{
    Task<UserProgress> GetProgressAsync();
    Task SaveProgressAsync(UserProgress progress);
    Task MarkLessonStartedAsync(string moduleId, string lessonId);
    Task MarkLessonCompletedAsync(string moduleId, string lessonId);
    Task MarkLabCompletedAsync(string moduleId, string lessonId, string labId);
    Task SaveQuizResultAsync(string moduleId, string lessonId, string quizId, int score, bool passed);
    Task<int> GetOverallProgressPercentageAsync(List<LearningModule> allModules);
    Task UpdateSettingsAsync(UserSettings settings);
    Task ResetProgressAsync();
    bool IsAuthenticated { get; }
    string? UserDisplayName { get; }
}

public class ProgressService : IProgressService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<ProgressService> _logger;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly CloudProgressService _cloudProgressService;
    private const string StorageKey = "ai_learning_progress";
    private UserProgress? _cachedProgress;
    private ClaimsPrincipal? _user;
    private bool _authStateInitialized;

    public ProgressService(
        IJSRuntime jsRuntime, 
        ILogger<ProgressService> logger,
        AuthenticationStateProvider authStateProvider,
        CloudProgressService cloudProgressService)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
        _authStateProvider = authStateProvider;
        _cloudProgressService = cloudProgressService;
    }

    public bool IsAuthenticated => _user?.Identity?.IsAuthenticated ?? false;
    
    public string? UserDisplayName => _user?.Identity?.Name ?? 
        _user?.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

    private string? UserId => _user?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    private string? UserEmail => _user?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

    private async Task EnsureAuthStateAsync()
    {
        if (!_authStateInitialized)
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            _user = authState.User;
            _authStateInitialized = true;
        }
    }

    public async Task<UserProgress> GetProgressAsync()
    {
        await EnsureAuthStateAsync();

        if (_cachedProgress != null)
            return _cachedProgress;

        try
        {
            // If authenticated, use cloud storage
            if (IsAuthenticated && !string.IsNullOrEmpty(UserId))
            {
                _cachedProgress = await _cloudProgressService.GetProgressAsync(UserId, UserEmail ?? "");
                return _cachedProgress;
            }

            // Otherwise, use local storage
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrEmpty(json))
            {
                _cachedProgress = JsonSerializer.Deserialize<UserProgress>(json) ?? new UserProgress();
            }
            else
            {
                _cachedProgress = new UserProgress();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading progress");
            _cachedProgress = new UserProgress();
        }

        return _cachedProgress;
    }

    public async Task SaveProgressAsync(UserProgress progress)
    {
        await EnsureAuthStateAsync();

        try
        {
            progress.LastActivityAt = DateTime.UtcNow;
            
            // If authenticated, save to cloud
            if (IsAuthenticated && !string.IsNullOrEmpty(UserId))
            {
                await _cloudProgressService.SaveProgressAsync(UserId, UserEmail ?? "", UserDisplayName ?? "", progress);
                _cachedProgress = progress;
                return;
            }

            // Otherwise, save to local storage
            var json = JsonSerializer.Serialize(progress);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
            _cachedProgress = progress;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving progress");
        }
    }

    public async Task MarkLessonStartedAsync(string moduleId, string lessonId)
    {
        var progress = await GetProgressAsync();
        
        // Ensure module progress exists
        if (!progress.ModuleProgress.ContainsKey(moduleId))
        {
            progress.ModuleProgress[moduleId] = new ModuleProgress
            {
                ModuleId = moduleId,
                IsStarted = true,
                StartedAt = DateTime.UtcNow
            };
        }

        var moduleProgress = progress.ModuleProgress[moduleId];
        if (!moduleProgress.IsStarted)
        {
            moduleProgress.IsStarted = true;
            moduleProgress.StartedAt = DateTime.UtcNow;
        }

        // Ensure lesson progress exists
        if (!moduleProgress.LessonProgress.ContainsKey(lessonId))
        {
            moduleProgress.LessonProgress[lessonId] = new LessonProgress
            {
                LessonId = lessonId,
                IsStarted = true,
                StartedAt = DateTime.UtcNow
            };
        }
        else if (!moduleProgress.LessonProgress[lessonId].IsStarted)
        {
            moduleProgress.LessonProgress[lessonId].IsStarted = true;
            moduleProgress.LessonProgress[lessonId].StartedAt = DateTime.UtcNow;
        }

        await SaveProgressAsync(progress);
    }

    public async Task MarkLessonCompletedAsync(string moduleId, string lessonId)
    {
        var progress = await GetProgressAsync();
        
        // Ensure structure exists
        await MarkLessonStartedAsync(moduleId, lessonId);
        progress = await GetProgressAsync();

        var lessonProgress = progress.ModuleProgress[moduleId].LessonProgress[lessonId];
        lessonProgress.IsCompleted = true;
        lessonProgress.CompletedAt = DateTime.UtcNow;

        await SaveProgressAsync(progress);
    }

    public async Task MarkLabCompletedAsync(string moduleId, string lessonId, string labId)
    {
        var progress = await GetProgressAsync();
        await MarkLessonStartedAsync(moduleId, lessonId);
        progress = await GetProgressAsync();

        var lessonProgress = progress.ModuleProgress[moduleId].LessonProgress[lessonId];
        lessonProgress.LabProgress = new LabProgress
        {
            LabId = labId,
            IsCompleted = true,
            CompletedAt = DateTime.UtcNow,
            AttemptsCount = (lessonProgress.LabProgress?.AttemptsCount ?? 0) + 1
        };

        await SaveProgressAsync(progress);
    }

    public async Task SaveQuizResultAsync(string moduleId, string lessonId, string quizId, int score, bool passed)
    {
        var progress = await GetProgressAsync();
        await MarkLessonStartedAsync(moduleId, lessonId);
        progress = await GetProgressAsync();

        var lessonProgress = progress.ModuleProgress[moduleId].LessonProgress[lessonId];
        
        var existingBestScore = lessonProgress.QuizProgress?.BestScore ?? 0;
        
        lessonProgress.QuizProgress = new QuizProgress
        {
            QuizId = quizId,
            IsPassed = passed || (lessonProgress.QuizProgress?.IsPassed ?? false),
            BestScore = Math.Max(score, existingBestScore),
            AttemptsCount = (lessonProgress.QuizProgress?.AttemptsCount ?? 0) + 1,
            LastAttemptAt = DateTime.UtcNow
        };

        if (passed)
        {
            lessonProgress.IsCompleted = true;
            lessonProgress.CompletedAt = DateTime.UtcNow;
        }

        await SaveProgressAsync(progress);
    }

    public async Task<int> GetOverallProgressPercentageAsync(List<LearningModule> allModules)
    {
        var progress = await GetProgressAsync();
        
        var totalLessons = allModules.Sum(m => m.Lessons.Count);
        if (totalLessons == 0) return 0;

        var completedLessons = 0;
        foreach (var module in allModules)
        {
            if (progress.ModuleProgress.TryGetValue(module.Id, out var moduleProgress))
            {
                completedLessons += moduleProgress.LessonProgress
                    .Count(lp => lp.Value.IsCompleted);
            }
        }

        return (int)((double)completedLessons / totalLessons * 100);
    }

    public async Task UpdateSettingsAsync(UserSettings settings)
    {
        var progress = await GetProgressAsync();
        progress.Settings = settings;
        await SaveProgressAsync(progress);
    }

    public async Task ResetProgressAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
            _cachedProgress = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting progress");
        }
    }
}
