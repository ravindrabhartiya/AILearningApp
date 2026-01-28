using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AILearningApp.Models;
using Microsoft.Extensions.Options;

namespace AILearningApp.Services;

/// <summary>
/// Service for interacting with Azure OpenAI API
/// </summary>
public interface IAzureOpenAIService
{
    Task<LabExecutionResult> SendChatCompletionAsync(
        string systemPrompt,
        string userMessage,
        Dictionary<string, object>? parameters = null);
    
    Task<LabExecutionResult> SendChatCompletionAsync(
        List<ChatMessage> messages,
        Dictionary<string, object>? parameters = null);
    
    bool IsConfigured { get; }
}

public class AzureOpenAIService : IAzureOpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly AzureOpenAIConfig _config;
    private readonly ILogger<AzureOpenAIService> _logger;

    public AzureOpenAIService(
        HttpClient httpClient,
        IOptions<AzureOpenAIConfig> config,
        ILogger<AzureOpenAIService> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
    }

    public bool IsConfigured => 
        !string.IsNullOrEmpty(_config.Endpoint) && 
        !string.IsNullOrEmpty(_config.ApiKey) && 
        !string.IsNullOrEmpty(_config.DeploymentName);

    public async Task<LabExecutionResult> SendChatCompletionAsync(
        string systemPrompt,
        string userMessage,
        Dictionary<string, object>? parameters = null)
    {
        var messages = new List<ChatMessage>
        {
            new() { Role = "system", Content = systemPrompt },
            new() { Role = "user", Content = userMessage }
        };

        return await SendChatCompletionAsync(messages, parameters);
    }

    public async Task<LabExecutionResult> SendChatCompletionAsync(
        List<ChatMessage> messages,
        Dictionary<string, object>? parameters = null)
    {
        if (!IsConfigured)
        {
            return new LabExecutionResult
            {
                IsSuccess = false,
                ErrorMessage = "Azure OpenAI is not configured. Please add your endpoint, API key, and deployment name in the Settings page."
            };
        }

        var startTime = DateTime.UtcNow;

        try
        {
            var request = new ChatCompletionRequest
            {
                Messages = messages,
                MaxTokens = GetParameter<int>(parameters, "max_tokens", 1000),
                Temperature = GetParameter<double>(parameters, "temperature", 0.7),
                TopP = GetParameter<double>(parameters, "top_p", 1.0),
                FrequencyPenalty = GetParameter<double>(parameters, "frequency_penalty", 0),
                PresencePenalty = GetParameter<double>(parameters, "presence_penalty", 0)
            };

            var url = $"{_config.Endpoint.TrimEnd('/')}/openai/deployments/{_config.DeploymentName}/chat/completions?api-version={_config.ApiVersion}";

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequest.Headers.Add("api-key", _config.ApiKey);
            httpRequest.Content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            _logger.LogInformation("Sending request to Azure OpenAI: {Url}", url);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Azure OpenAI error: {StatusCode} - {Content}", 
                    response.StatusCode, responseContent);

                return new LabExecutionResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"API Error ({response.StatusCode}): {ParseErrorMessage(responseContent)}",
                    ExecutionTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds
                };
            }

            var result = JsonSerializer.Deserialize<ChatCompletionResponse>(responseContent);

            if (result?.Choices == null || result.Choices.Count == 0)
            {
                return new LabExecutionResult
                {
                    IsSuccess = false,
                    ErrorMessage = "No response generated from the model.",
                    ExecutionTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds
                };
            }

            return new LabExecutionResult
            {
                IsSuccess = true,
                Response = result.Choices[0].Message?.Content ?? string.Empty,
                TokenUsage = result.Usage,
                ExecutionTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds,
                Metadata = new Dictionary<string, object>
                {
                    { "model", result.Model },
                    { "finish_reason", result.Choices[0].FinishReason }
                }
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling Azure OpenAI");
            return new LabExecutionResult
            {
                IsSuccess = false,
                ErrorMessage = $"Network error: {ex.Message}",
                ExecutionTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds
            };
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout calling Azure OpenAI");
            return new LabExecutionResult
            {
                IsSuccess = false,
                ErrorMessage = "Request timed out. The model might be processing a complex request.",
                ExecutionTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Azure OpenAI");
            return new LabExecutionResult
            {
                IsSuccess = false,
                ErrorMessage = $"Unexpected error: {ex.Message}",
                ExecutionTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds
            };
        }
    }

    private static T GetParameter<T>(Dictionary<string, object>? parameters, string key, T defaultValue)
    {
        if (parameters == null || !parameters.TryGetValue(key, out var value))
            return defaultValue;

        try
        {
            if (value is JsonElement jsonElement)
            {
                if (typeof(T) == typeof(int))
                    return (T)(object)jsonElement.GetInt32();
                if (typeof(T) == typeof(double))
                    return (T)(object)jsonElement.GetDouble();
                if (typeof(T) == typeof(string))
                    return (T)(object)(jsonElement.GetString() ?? defaultValue?.ToString() ?? "");
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    private static string ParseErrorMessage(string responseContent)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseContent);
            if (doc.RootElement.TryGetProperty("error", out var errorElement))
            {
                if (errorElement.TryGetProperty("message", out var messageElement))
                {
                    return messageElement.GetString() ?? "Unknown error";
                }
            }
        }
        catch
        {
            // If parsing fails, return the raw content
        }
        return responseContent.Length > 200 ? responseContent[..200] + "..." : responseContent;
    }
}
