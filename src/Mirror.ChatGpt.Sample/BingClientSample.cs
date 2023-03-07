﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mirror.ChatGpt.Sample;

internal class BingClientSample
{
    public async Task ChatAsync()
    {
        var services = new ServiceCollection();

        //just show debug message. If you want to trace or diagnose your conversation please remove this comments
        //services.AddLogging(x => { x.AddConsole(); });

        const string token = ""; //Cookie of Microsoft account which named _U
        //Register services
        services.AddBingClient(new()
        {
            Token = token
        });
        var app = services.BuildServiceProvider();

        var service = app.GetRequiredService<BingClient>();
        //Use this event to Receive realtime message
        service.MessageReceived += (sender, e) =>
        {
            if (e.Begin)
                Console.Write($"[{DateTime.Now:HH:mm:ss} Bing] ");
            Console.Write(e.Text);
            if (e.End)
            {
                Console.WriteLine();
                Console.WriteLine("-------------------------------");
            }
        };
        var invocationId = 0;
        const int maxConversationCount = 6;
        Console.WriteLine("Let's start,input 'exit' to escape and 'reset' to create a new conversation");
        Console.WriteLine();
        do
        {
            Console.WriteLine("-------------------------------");
            Console.Write($"[{DateTime.Now:HH:mm:ss} You] ");
            var text = Console.ReadLine();
            if (text== "exit")
                break;
            if (text == "reset")
            {
                invocationId = 0;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss} System]  Conversation reset.");
                continue;
            }
            Console.WriteLine();

            var chatCts = new CancellationTokenSource();
            //Set timeout by CancellationTokenSource
            chatCts.CancelAfter(TimeSpan.FromMinutes(5));

            //This method will return final message
            var res = await service.ChatAsync(new(text)
            {
                InvocationId = invocationId
            }, chatCts.Token);
            invocationId = res.InvocationId;
            if (invocationId == 0)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}System] Bing closed the conversation.");
                continue;
            }
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss} System] The final-> {invocationId}/{maxConversationCount}");
            Console.WriteLine($"<{res.Text}>");
            if (maxConversationCount <= invocationId)
            {
                Console.WriteLine("-------------------------------");
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}System] Conversation count limited,it will be reset at next one.");
                Console.WriteLine("-------------------------------");
                invocationId = 0;
            }
        } while (true);
    }
}