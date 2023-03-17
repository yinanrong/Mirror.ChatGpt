namespace Mirror.ChatGpt.Models.Bing;

public record BingClientOptions
{
    public double TimeoutSeconds { get; set; } = 120;
    public string Token { get; set; }
    public string Proxy { get; set; }
}