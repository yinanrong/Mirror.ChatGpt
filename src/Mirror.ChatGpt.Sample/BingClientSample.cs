using Microsoft.Extensions.DependencyInjection;

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
        services.AddBingClient(new() { Token = token });
        var app = services.BuildServiceProvider();

        var service = app.GetRequiredService<BingClient>();
        //Use this event to Receive real time message
        service.MessageReceived += (sender, e) =>
        {
            if (e.Begin)
                Console.Write($"{DateTime.Now:HH:mm:ss} Bing :");
            Console.Write(e.Text);
            if (e.End)
                Console.WriteLine("-------------------------------");
        };
        var invocationId = 0;
        Console.WriteLine("Let's start，input 'exit' to escape");
        do
        {
            Console.WriteLine("-------------------------------");
            Console.Write($"{DateTime.Now:HH:mm:ss} You :");
            var text = Console.ReadLine();
            if (text is null or "exit")
                break;
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
            Console.WriteLine();
            if (res.Success)
                Console.WriteLine($"[finally:{res.Text}]");
        } while (true);
    }
}