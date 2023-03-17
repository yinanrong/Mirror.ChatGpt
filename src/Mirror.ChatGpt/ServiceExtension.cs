using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Mirror.ChatGpt.Models.Bing;
using Mirror.ChatGpt.Models.ChatGpt;

namespace Mirror.ChatGpt;

public static class ServiceExtension
{
    public static IServiceCollection AddChatGptClient(this IServiceCollection services, ChatGptClientOptions options)
    {
        services.AddHttpClient("chatgpt",x=>x.Timeout=TimeSpan.FromSeconds(options.TimeoutSeconds))
            .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
            if (!string.IsNullOrEmpty(options.Proxy))
                handler.Proxy =new WebProxy(options.Proxy);
            return handler;
        });
        services.AddSingleton(options);
        services.AddScoped<ChatGptClient>();
        return services;
    }

    public static IServiceCollection AddBingClient(this IServiceCollection services, BingClientOptions options)
    {
        services.AddHttpClient("bing", x => x.Timeout=TimeSpan.FromSeconds(options.TimeoutSeconds))
            .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
            if (!string.IsNullOrEmpty(options.Proxy))
                handler.Proxy =new WebProxy(options.Proxy);
            return handler;
        });
        services.AddSingleton(options);
        services.AddScoped<BingClient>();
        return services;
    }
}