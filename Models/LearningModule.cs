namespace AILearningApp.Models;

/// <summary>
/// Represents a learning module (e.g., "LLM Fundamentals", "Prompt Engineering")
/// </summary>
public class LearningModule
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-book"; // Bootstrap icon
    public DifficultyLevel Level { get; set; }
    public int Order { get; set; }
    public List<Lesson> Lessons { get; set; } = new();
    public List<string> Prerequisites { get; set; } = new(); // Module IDs
    public int EstimatedMinutes { get; set; }
}

/// <summary>
/// Represents a lesson within a module
/// </summary>
public class Lesson
{
    public string Id { get; set; } = string.Empty;
    public string ModuleId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public LessonType Type { get; set; }
    public List<ContentSection> Sections { get; set; } = new();
    public Lab? Lab { get; set; }
    public Quiz? Quiz { get; set; }
    public int EstimatedMinutes { get; set; }
}

/// <summary>
/// Represents a content section within a lesson
/// </summary>
public class ContentSection
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // Markdown content
    public ContentType Type { get; set; }
    public int Order { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Represents an interactive lab
/// </summary>
public class Lab
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty; // Markdown
    public LabType LabType { get; set; }
    public string StarterCode { get; set; } = string.Empty;
    public string ExpectedOutput { get; set; } = string.Empty;
    public List<LabHint> Hints { get; set; } = new();
    public string SystemPrompt { get; set; } = string.Empty; // For AOI labs
    public Dictionary<string, object> Parameters { get; set; } = new(); // AOI parameters
}

/// <summary>
/// Represents a hint for a lab
/// </summary>
public class LabHint
{
    public int Order { get; set; }
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Represents a quiz for assessment
/// </summary>
public class Quiz
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<QuizQuestion> Questions { get; set; } = new();
    public int PassingScore { get; set; } = 70; // Percentage
}

/// <summary>
/// Represents a quiz question
/// </summary>
public class QuizQuestion
{
    public string Id { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public List<QuizOption> Options { get; set; } = new();
    public string Explanation { get; set; } = string.Empty;
    public int Points { get; set; } = 1;
}

/// <summary>
/// Represents a quiz option
/// </summary>
public class QuizOption
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

// Enums
public enum DifficultyLevel
{
    Novice,
    Beginner,
    Intermediate,
    Advanced,
    Expert
}

public enum LessonType
{
    Theory,
    Tutorial,
    Lab,
    Assessment
}

public enum ContentType
{
    Text,
    Code,
    Image,
    Video,
    Interactive,
    Diagram
}

public enum LabType
{
    PromptEngineering,
    ApiCall,
    Comparison,
    FreeForm,
    CodeCompletion
}

public enum QuestionType
{
    SingleChoice,
    MultipleChoice,
    TrueFalse,
    FillInBlank
}
