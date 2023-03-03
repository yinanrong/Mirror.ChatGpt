namespace Mirror.ChatGpt.Models.Bing;

public record BingClientOptions
{
    public string Token { get; set; }
    public string Proxy { get; set; }
}