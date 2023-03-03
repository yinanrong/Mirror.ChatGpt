namespace Mirror.ChatGpt.Models.Bing;

internal record GetConversationResponse
{
    public string ConversationId { get; set; }
    public string ClientId { get; set; }
    public string ConversationSignature { get; set; }
    public ConversationResultValue Result { get; set; }

    public record ConversationResultValue
    {
        public string Value { get; set; }
        public string Message { get; set; }
    }
}