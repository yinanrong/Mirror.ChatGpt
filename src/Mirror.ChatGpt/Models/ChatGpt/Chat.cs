using Newtonsoft.Json;

namespace Mirror.ChatGpt.Models.ChatGpt;
public class ChatCompletionRequest
{
    public ChatCompletionRequest()
    {
    }

    public ChatCompletionRequest(string model, MessageEntry[] messages)
    {
        Model = model;
        Messages = messages;
    }

    public string Model { get; set; }
    public MessageEntry[] Messages { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public float? Temperature { get; set; }

    [JsonProperty("top_p", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public float? TopP { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int? N { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool? Stream { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string[] Stop { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int? MaxTokens { get; set; }

    [JsonProperty("presence_penalty", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public float? PresencePenalty { get; set; }

    [JsonProperty("frequency_penalty", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public float? FrequencyPenalty { get; set; }


    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string[] Prefix { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string[] User { get; set; }

    [JsonProperty("model_context", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string[] ModelContext { get; set; }

    [JsonProperty("model_response", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string[] ModelResponse { get; set; }

    [JsonProperty("logit_bias", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public IDictionary<string, object> LogitBias { get; set; }
}

public class ChatCompletionResponse
{
    public string Id { get; set; }
    public string Object { get; set; }
    public int Created { get; set; }
    public Choice[] Choices { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public UsageEntry Usage { get; set; }


    public class Choice
    {
        public int Index { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public MessageEntry Message { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public MessageEntry Delta { get; set; }

        [JsonProperty("finish_reason")] public string FinishReason { get; set; }
    }

    public class UsageEntry
    {
        [JsonProperty("prompt_tokens")] public int PromptTokens { get; set; }

        [JsonProperty("completion_tokens")] public int CompletionTokens { get; set; }

        [JsonProperty("total_tokens")] public int TotalTokens { get; set; }
    }
}

public class MessageEntry
{
    public string Role { get; set; }
    public string Content { get; set; }
}

public static class Roles
{
    public const string System = "system";
    public const string Assistant = "assistant";
    public const string User = "user";
}

internal class ErrorResponse
{
    public ErrorEntry Error { get; set; }

    internal class ErrorEntry
    {
        public string Message { get; set; }
        public string Type { get; set; }
        public string Param { get; set; }
        public string Code { get; set; }
    }
}