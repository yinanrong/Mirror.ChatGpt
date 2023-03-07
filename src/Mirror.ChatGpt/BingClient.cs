using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Mirror.ChatGpt.Models.Bing;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Mirror.ChatGpt;

public class BingClient
{
    private readonly ChatExtension _extension;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly BingClientOptions _options;
    private readonly JsonSerializerSettings _serializerSettings;

    public BingClient(BingClientOptions options, ILogger<BingClient> logger, IHttpClientFactory httpClientFactory)
    {
        _options = options;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _serializerSettings = new()
        {
            Formatting = Formatting.None,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        _extension = new();
    }

    public event EventHandler<ChatPressResponse> MessageReceived;

    public async Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken cancellationToken)
    {
        var isStartOfSession = request.InvocationId <= 0;
        if (isStartOfSession)
        {
            var conversation = await CreateConversationAsync(_options.Token, cancellationToken);
            if (conversation?.Result?.Value != "Success")
                throw new($"create conversation error:{conversation?.Result?.Message}");
            _extension.ConversationId = conversation.ConversationId;
            _extension.ClientId = conversation.ClientId;
            _extension.ConversationSignature = conversation.ConversationSignature;
        }

        var chatRequest = new InternalChatRequest(request.InvocationId.ToString(), new()
        {
            new(isStartOfSession, _extension.ConversationId,
                _extension.ConversationSignature, new(_extension.ClientId),
                new(request.Text))
        });
        using var ws = await CreateConnectionAsync(cancellationToken);
        await HandshakeAsync(ws, cancellationToken);
        await SendMessageAsync(ws, chatRequest, cancellationToken);
        var res = await ReceiveAsync(ws, cancellationToken);
        var invocationId = res == null ? 0 : request.InvocationId+1;
        return new(invocationId, res);
    }

    private async Task HandshakeAsync(WebSocket ws, CancellationToken cancellationToken)
    {
        _logger.LogDebug("performing handshake...");
        await SendMessageAsync(ws, new
        {
            protocol = "json",
            version = 1
        }, cancellationToken);
        var res = await ReceiveAsync(ws, cancellationToken);
        if (res == "{}")
        {
            _logger.LogDebug("handshake established");
            await SendMessageAsync(ws, new
            {
                type = 6
            }, cancellationToken);
            _logger.LogDebug("heart beat message sent");
            return;
        }

        throw new($"handshake error:{res}");
    }

    private async Task<string> ReceiveAsync(WebSocket ws, CancellationToken cancellationToken)
    {
        var lastText = "";
        var i = 0;
        _logger.LogDebug("ready to receive message..");
        while (ws.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            WebSocketReceiveResult result;
            var messages = new StringBuilder();
            do
            {
                var buffer = new byte[1024];
                result = await ws.ReceiveAsync(buffer, cancellationToken);
                var messageJson = Encoding.UTF8.GetString(buffer.Take(result.Count).ToArray());
                messages.Append(messageJson);
            } while (!result.EndOfMessage &&
                     result.MessageType == WebSocketMessageType.Text && !cancellationToken.IsCancellationRequested);

            var objects = messages.ToString().Split("\u001e", StringSplitOptions.RemoveEmptyEntries);
            if (objects.Length <= 0)
                continue;
            var responseMsg = objects[0];
            _logger.LogDebug($"receiving general message:{responseMsg}");
            var response = JsonConvert.DeserializeObject<InternalChatResponse>(responseMsg)!;
            switch (response.Type)
            {
                case 1:
                    if (response is not { Arguments.Count: > 0 } || response.Arguments[0] is not { Messages.Count: > 0 })
                        continue;
                    var message = response.Arguments[0].Messages[0];
                    if (message.Author != "bot" || string.IsNullOrEmpty(message.Text))
                        continue;
                    message.Text = Regex
                        .Replace(message.Text, @"\[[^\]]+\]", "", RegexOptions.Compiled|RegexOptions.Multiline)
                        .Replace("\ud83d\ude0a","");
                    var thisText = message.Text.Length >= lastText.Length
                        ? message.Text[lastText.Length..]
                        : message.Text;
                    lastText = message.Text;
                    var init = i++ <= 0;
                    var end = thisText == "";
                    MessageReceived?.Invoke(this, new(init, end, thisText));
                    if (end)
                    {
                        _logger.LogDebug("receiving text message successfully");
                        return string.IsNullOrEmpty(message.Text) ? lastText : message.Text;
                    }
                    break;
                case 0:
                    return responseMsg;
                case 2:
                    return null;
                case 6:
                    await SendMessageAsync(ws, new
                    {
                        type = 6
                    }, cancellationToken);
                    continue;
                default:
                    continue;
            }
        }

        return null;
    }

    private async Task SendMessageAsync(WebSocket ws, object msg, CancellationToken cancellationToken)
    {
        var jsonMessage = JsonConvert.SerializeObject(msg, _serializerSettings) + "\u001e";
        var bytesMessage = Encoding.UTF8.GetBytes(jsonMessage);
        await ws.SendAsync(bytesMessage, WebSocketMessageType.Text, true, cancellationToken);
    }

    private async Task<WebSocket> CreateConnectionAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("creating new connection...");
        var ws = new ClientWebSocket();
        var uri = new Uri("wss://sydney.bing.com/sydney/ChatHub");
        await ws.ConnectAsync(uri, cancellationToken);
        _logger.LogDebug("new connection created");
        return ws;
    }

    private async Task<GetConversationResponse> CreateConversationAsync(string token,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("creating new conversation...");
        var headers = new Dictionary<string, string>
        {
            {"accept", "application/json"},
            {"accept-language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6"},
            {"content-type", "application/json"},
            {"sec-ch-ua", "\"Not_A Brand\";v=\"99\", \"Microsoft Edge\";v=\"109\", \"Chromium\";v=\"109\""},
            {"sec-ch-ua-arch", "\"x86\""},
            {"sec-ch-ua-bitness", "\"64\""},
            {"sec-ch-ua-full-version", "\"109.0.1518.78\""},
            {
                "sec-ch-ua-full-version-list",
                "\"Not_A Brand\";v=\"99.0.0.0\", \"Microsoft Edge\";v=\"109.0.1518.78\", \"Chromium\";v=\"109.0.5414.120\""
            },
            {"sec-ch-ua-mobile", "?0"},
            {"sec-ch-ua-model", ""},
            {"sec-ch-ua-platform", "\"Windows\""},
            {"sec-ch-ua-platform-version", "\"15.0.0\""},
            {"sec-fetch-dest", "empty"},
            {"sec-fetch-mode", "cors"},
            {"sec-fetch-site", "same-origin"},
            {"x-ms-client-request-id", Guid.NewGuid().ToString()},
            {"x-ms-useragent", "azsdk-js-api-client-factory/1.0.0-beta.1 core-rest-pipeline/1.10.0 OS/Win32"},
            {"cookie", $"_U={token}"},
            {"Referer", "https://www.bing.com/search?q=Bing+AI&showconv=1&FORM=hpcodx"},
            {"Referrer-Policy", "origin-when-cross-origin"},
            {
                "user-agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36 Edg/110.0.1587.57"
            }
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/turing/conversation/create");

        var httpClient = _httpClientFactory.CreateClient("bing");
        httpClient.DefaultRequestHeaders.Clear();
        foreach (var header in headers) request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        httpClient.BaseAddress = new("https://www.bing.com");
        var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogDebug($"new conversation created:{responseBody}");
        return JsonConvert.DeserializeObject<GetConversationResponse>(responseBody);
    }
}