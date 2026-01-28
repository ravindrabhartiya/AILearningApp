namespace AILearningApp.Models;

/// <summary>
/// Tracks user progress through the learning content
/// Stored in browser local storage
/// </summary>
public class UserProgress
{
    public string UserId { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, ModuleProgress> ModuleProgress { get; set; } = new();
    public List<Achievement> Achievements { get; set; } = new();
    public UserSettings Settings { get; set; } = new();
}

/// <summary>
/// Progress within a specific module
/// </summary>
public class ModuleProgress
{
    public string ModuleId { get; set; } = string.Empty;
    public bool IsStarted { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Dictionary<string, LessonProgress> LessonProgress { get; set; } = new();
    public int TotalTimeSpentMinutes { get; set; }
}

/// <summary>
/// Progress within a specific lesson
/// </summary>
public class LessonProgress
{
    public string LessonId { get; set; } = string.Empty;
    public bool IsStarted { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<string> CompletedSections { get; set; } = new();
    public LabProgress? LabProgress { get; set; }
    public QuizProgress? QuizProgress { get; set; }
}

/// <summary>
/// Progress for a lab exercise
/// </summary>
public class LabProgress
{
    public string LabId { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int AttemptsCount { get; set; }
    public List<string> HintsUsed { get; set; } = new();
    public string LastSubmission { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// Progress for a quiz
/// </summary>
public class QuizProgress
{
    public string QuizId { get; set; } = string.Empty;
    public bool IsPassed { get; set; }
    public int BestScore { get; set; }
    public int AttemptsCount { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public Dictionary<string, string> LastAnswers { get; set; } = new();
}

/// <summary>
/// User achievement/badge
/// </summary>
public class Achievement
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public DateTime EarnedAt { get; set; }
}

/// <summary>
/// User preferences and settings
/// </summary>
public class UserSettings
{
    public string DisplayName { get; set; } = "Learner";
    public string Theme { get; set; } = "auto"; // auto, light, dark
    public bool ShowCodeLineNumbers { get; set; } = true;
    public string PreferredCodeTheme { get; set; } = "vs-dark";
    public bool EnableAnimations { get; set; } = true;
}
