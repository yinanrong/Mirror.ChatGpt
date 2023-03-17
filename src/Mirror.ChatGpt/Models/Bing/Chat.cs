namespace Mirror.ChatGpt.Models.Bing;

public record ChatRequest(string Text)
{
    public ChatExtension ChatExtension { get; set; }
}

public record ChatResponse(string Text, ChatExtension ChatExtension);

public record ChatExtension(int InvocationId, string ConversationId, string ClientId, string ConversationSignature);
internal record InternalChatRequest(string InvocationId, List<InternalChatRequest.Argument> Arguments)
{
    public string Target => "chat";
    public int Type => 4;

    #region inner class

    public record Message(string Text)
    {
        public string Author => "user";
        public string InputMethod => "Keyboard";
        public string MessageType => "Chat";
    }

    public record Participant(string Id);

    public record Argument(bool IsStartOfSession, string ConversationId, string ConversationSignature,
        Participant Participant, Message Message)
    {
        public string Source => "cib";

        public IEnumerable<string> OptionsSets => new[]
        {
            "nlu_direct_response_filter",
            "deepleo",
            "enable_debug_commands",
            "disable_emoji_spoken_text",
            "responsible_ai_policy_235",
            "enablemm"
        };
    }

    #endregion
}

internal record InternalChatResponse
{
    public int Type { get; set; }
    public string Target { get; set; }
    public List<Argument> Arguments { get; set; }

    #region inner class

    public class AdaptiveCard
    {
        public string Type { get; set; }
        public string Version { get; set; }
        public List<TextBlock> Body { get; set; }
    }

    public class TextBlock
    {
        public string Type { get; set; }
        public string Text { get; set; }
        public bool Wrap { get; set; }
    }

    public class Feedback
    {
        public object Tag { get; set; }
        public object UpdatedOn { get; set; }
        public string Type { get; set; }
    }

    public class Message
    {
        public string Text { get; set; }
        public string Author { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string MessageId { get; set; }
        public string Offense { get; set; }
        public List<AdaptiveCard> AdaptiveCards { get; set; }
        public List<object> SourceAttributions { get; set; }
        public Feedback Feedback { get; set; }
        public string ContentOrigin { get; set; }
        public object Privacy { get; set; }
    }

    public class Argument
    {
        public List<Message> Messages { get; set; }
        public string RequestId { get; set; }
    }

    #endregion
}