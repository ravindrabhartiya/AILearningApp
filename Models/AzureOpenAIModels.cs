using System.Text.Json.Serialization;

namespace AILearningApp.Models;

/// <summary>
/// Configuration for Azure OpenAI connection
/// </summary>
public class AzureOpenAIConfig
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "2024-08-01-preview";
}

/// <summary>
/// Request to Azure OpenAI Chat Completions API
/// </summary>
public class ChatCompletionRequest
{
    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; } = new();

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; } = 1000;

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.7;

    [JsonPropertyName("top_p")]
    public double TopP { get; set; } = 1.0;

    [JsonPropertyName("frequency_penalty")]
    public double FrequencyPenalty { get; set; } = 0;

    [JsonPropertyName("presence_penalty")]
    public double PresencePenalty { get; set; } = 0;

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;
}

/// <summary>
/// Chat message
/// </summary>
public class ChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "user"; // system, user, assistant

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Response from Azure OpenAI Chat Completions API
/// </summary>
public class ChatCompletionResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("choices")]
    public List<ChatChoice> Choices { get; set; } = new();

    [JsonPropertyName("usage")]
    public UsageInfo? Usage { get; set; }
}

/// <summary>
/// Chat completion choice
/// </summary>
public class ChatChoice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public ChatMessage? Message { get; set; }

    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; } = string.Empty;
}

/// <summary>
/// Token usage information
/// </summary>
public class UsageInfo
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

/// <summary>
/// Lab execution result
/// </summary>
public class LabExecutionResult
{
    public bool IsSuccess { get; set; }
    public string Response { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public UsageInfo? TokenUsage { get; set; }
    public double ExecutionTimeMs { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
