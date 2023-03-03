using System.Text;
using Mirror.ChatGpt.Models.ChatGpt;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Mirror.ChatGpt;

public class ChatGptClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ChatGptClientOptions _options;
    private readonly JsonSerializerSettings _serializerSettings;
    public ChatGptClient(ChatGptClientOptions options, IHttpClientFactory httpClientFactory)
    {
        _options = options;
        _httpClientFactory = httpClientFactory;
        _serializerSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
    }

    public ChatGptClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ChatCompletionResponse> ChatAsync(ChatCompletionRequest request,
        CancellationToken cancellationToken)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/chat/completions")
        {
            Content = new StringContent(JsonConvert.SerializeObject(request, _serializerSettings), Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Add("Authorization", $"Bearer {_options.ApiKey}");
        if (!string.IsNullOrEmpty(_options.Organization))
            httpRequest.Headers.Add("OpenAI-Organization", _options.Organization);
        var httpClient = _httpClientFactory.CreateClient("chatgpt");
        httpClient.BaseAddress = new("https://api.openai.com");
        var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
        if (response.IsSuccessStatusCode)
            return JsonConvert.DeserializeObject<ChatCompletionResponse>(responseText);
        var error = JsonConvert.DeserializeObject<ErrorResponse>(responseText);
        throw new(error?.Error?.Message);
    }
}