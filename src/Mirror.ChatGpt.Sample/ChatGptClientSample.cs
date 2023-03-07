using Microsoft.Extensions.DependencyInjection;
using Mirror.ChatGpt.Models.ChatGpt;
using Newtonsoft.Json.Linq;

namespace Mirror.ChatGpt.Sample;

internal class ChatGptClientSample
{
    public async Task ChatAsync()
    {
        var services = new ServiceCollection();

        //just show debug message. If you want to trace or diagnose your conversation please remove this comments
        //services.AddLogging(x => { x.AddConsole(); });

        //Register services
        services.AddChatGptClient(new()
        {
            ApiKey = "",//Your api key from OpenAI
            Organization = "",//Your organization from OpenAi,optional
            Proxy = "http://127.0.0.1:7890" //proxy address,optional
        });
        var app = services.BuildServiceProvider();

        var service = app.GetRequiredService<ChatGptClient>();
        ChatCompletionRequest request = new()
        {
            Stream = true, //receive realtime message
            Model = "gpt-3.5-turbo", //model name,required. only gpt-3.5-turbo or gpt-3.5-turbo-0301 can be chosen now
            Messages = //message list
                new[]
                {
                    new MessageEntry
                    {
                        Role = Roles.System,
                        Content = "You are a helpful assistant."
                    },
                    new MessageEntry
                    {
                        Role = Roles.User,
                        Content = "Who won the world series in 2020?"
                    },
                    new MessageEntry
                    {
                        Role = Roles.Assistant,
                        Content = "The Los Angeles Dodgers won the World Series in 2020."
                    },
                    new MessageEntry
                    {
                        Role = Roles.User,
                        Content = "Where was it played?"
                    }
                }
        };

        if (request.Stream==true)
            service.MessageReceived += (send, e) =>
            {
                Console.Write(e.Text);
                if (e.End)
                {
                    Console.WriteLine();
                    Console.WriteLine("-------------------------------");
                }
            };
        var res = await service.ChatAsync(request, default);

        Console.WriteLine($"final:{res.Choices[0].Message.Content}");
    }
}