namespace Mirror.ChatGpt.Models.ChatGpt;

public class ChatGptClientOptions
{
    public double TimeoutSeconds { get; set; } = 120;
    public string ApiKey { get; set; }
    public string Organization { get; set; }
    public string Proxy { get; set; }
}