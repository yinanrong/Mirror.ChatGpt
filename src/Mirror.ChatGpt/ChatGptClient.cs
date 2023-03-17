using System.Text;
using Mirror.ChatGpt.Models;
using Mirror.ChatGpt.Models.ChatGpt;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using static Mirror.ChatGpt.Models.ChatGpt.ChatCompletionResponse;

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

    public event EventHandler<ChatPressResponse> MessageReceived;

    public async Task<ChatCompletionResponse> ChatAsync(ChatCompletionRequest request,
        CancellationToken cancellationToken)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/chat/completions")
        {
            Content = new StringContent(JsonConvert.SerializeObject(request, _serializerSettings), Encoding.UTF8,
                "application/json")
        };
        httpRequest.Headers.Add("Authorization", $"Bearer {_options.ApiKey}");
        if (!string.IsNullOrEmpty(_options.Organization))
            httpRequest.Headers.Add("OpenAI-Organization", _options.Organization);
        var httpClient = _httpClientFactory.CreateClient("chatgpt");
        httpClient.BaseAddress = new("https://api.openai.com");
        var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
            var error = JsonConvert.DeserializeObject<ErrorResponse>(errorText);
            throw new(error?.Error?.Message);
        }

        if (request.Stream == true)
        {
            var allText = new StringBuilder();
            string finishReason = null;
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);
            do
            {
                var line = await reader.ReadLineAsync();

                if (string.IsNullOrEmpty(line))
                    continue;
                line=line.Remove(0, 6);//remove "data: "
                if (line == "[DONE]")
                    break;
                var token = JsonConvert.DeserializeObject<ChatCompletionResponse>(line);
                if (token is not { Choices.Length: > 0 })
                    break;
                var choice = token.Choices[0];
                var begin = choice.Delta?.Role != null;
                var end = !begin&&(choice.FinishReason != null || choice.Delta?.Content == null);
                MessageReceived?.Invoke(this, new(begin, end, choice.Delta?.Content));
                if (end)
                {
                    finishReason = choice.FinishReason;
                    break;
                }

                allText.Append(choice.Delta?.Content);
            } while (!cancellationToken.IsCancellationRequested && !reader.EndOfStream);

            return new()
            {
                Choices = new Choice[]
                {
                    new()
                    {
                        FinishReason = finishReason,
                        Message = new()
                        {
                            Content = allText.ToString(),
                            Role = Roles.Assistant
                        }
                    }
                }
            };
        }

        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<ChatCompletionResponse>(responseText);
    }
}