using AILearningApp.Models;
using Azure;
using Azure.Data.Tables;
using System.Text.Json;

namespace AILearningApp.Services;

/// <summary>
/// Entity for storing user progress in Azure Table Storage
/// </summary>
public class UserProgressEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ProgressJson { get; set; } = string.Empty;
    public DateTime LastActivityAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Cloud-based progress service using Azure Table Storage for authenticated users
/// </summary>
public class CloudProgressService
{
    private readonly TableClient _tableClient;
    private readonly ILogger<CloudProgressService> _logger;
    private const string TableName = "UserProgress";

    public CloudProgressService(IConfiguration configuration, ILogger<CloudProgressService> logger)
    {
        _logger = logger;
        var connectionString = configuration["AzureStorage:ConnectionString"];
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Azure Storage connection string is not configured");
        }

        var serviceClient = new TableServiceClient(connectionString);
        _tableClient = serviceClient.GetTableClient(TableName);
        _tableClient.CreateIfNotExists();
    }

    public async Task<UserProgress> GetProgressAsync(string userId, string email)
    {
        try
        {
            var response = await _tableClient.GetEntityIfExistsAsync<UserProgressEntity>("users", userId);
            
            if (response.HasValue && response.Value != null)
            {
                var entity = response.Value;
                if (!string.IsNullOrEmpty(entity.ProgressJson))
                {
                    return JsonSerializer.Deserialize<UserProgress>(entity.ProgressJson) ?? new UserProgress();
                }
            }
            
            // Return new progress for new users
            return new UserProgress();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading progress from Azure Table Storage for user {UserId}", userId);
            return new UserProgress();
        }
    }

    public async Task SaveProgressAsync(string userId, string email, string displayName, UserProgress progress)
    {
        try
        {
            progress.LastActivityAt = DateTime.UtcNow;
            var progressJson = JsonSerializer.Serialize(progress);

            var entity = new UserProgressEntity
            {
                PartitionKey = "users",
                RowKey = userId,
                Email = email,
                DisplayName = displayName,
                ProgressJson = progressJson,
                LastActivityAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving progress to Azure Table Storage for user {UserId}", userId);
        }
    }

    public async Task<List<(string Email, string DisplayName, int CompletedLessons, DateTime LastActivity)>> GetLeaderboardAsync(int top = 10)
    {
        var leaderboard = new List<(string Email, string DisplayName, int CompletedLessons, DateTime LastActivity)>();

        try
        {
            await foreach (var entity in _tableClient.QueryAsync<UserProgressEntity>(e => e.PartitionKey == "users"))
            {
                if (!string.IsNullOrEmpty(entity.ProgressJson))
                {
                    var progress = JsonSerializer.Deserialize<UserProgress>(entity.ProgressJson);
                    if (progress != null)
                    {
                        var completedCount = progress.ModuleProgress.Values
                            .SelectMany(m => m.LessonProgress.Values)
                            .Count(l => l.IsCompleted);

                        leaderboard.Add((entity.Email, entity.DisplayName, completedCount, entity.LastActivityAt));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching leaderboard");
        }

        return leaderboard
            .OrderByDescending(x => x.CompletedLessons)
            .ThenByDescending(x => x.LastActivity)
            .Take(top)
            .ToList();
    }
}
