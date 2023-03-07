# Mirror.ChatGPT

This project provides official API for ChatGPT and unofficial API for Bing chat in C# language. It requires .NET 6 or above. The license is Apache 2.0.

Bing chat is free

## Usage

### Bing Chat

**To use the Bing chat API, you must have a cookie for a Microsoft account named "_U". You can then use the following code to start a chat:**

#### basic
``` c#
    var services = new ServiceCollection();
    //services.AddLogging(x => { x.AddConsole(); });

    const string token = "xxx"; //Cookie of Microsoft account which named _U
    //Register services
    services.AddBingClient(new() {Token = token});
    var app = services.BuildServiceProvider();

    var service = app.GetRequiredService<BingClient>();
    var chatCts = new CancellationTokenSource();
        
    //Set timeout by CancellationTokenSource
    chatCts.CancelAfter(TimeSpan.FromMinutes(5));
    
    var res = await service.ChatAsync(new("hello"), chatCts.Token);
    if (res.Success)
        Console.WriteLine($"[finally:{res.Text}]");
```

#### receive real time message
``` c#
    var service = app.GetRequiredService<BingClient>();
    //This event will respond every received token 
    service.MessageReceived += (sender, e) =>
    {
        if (e.Begin)
            Console.Write($"{DateTime.Now:HH:mm:ss} Bing :");
        Console.Write(e.Text);
        if (e.End)
            Console.WriteLine("-------------------------------");
    };
 ```
### ChatGPT
**To use the ChatGPT API, you must have an API key from OpenAI. You can then use the following code to start a chat:**

Note that you must pass back the content of each conversation to the Messages object to maintain a coherent chat context.
```C#
    var services = new ServiceCollection();
    //Register services
    services.AddChatGptClient(new()
    {
        ApiKey = "",//Your api key from OpenAI
        Organization = "",//Your organization from OpenAi,optional
        Proxy = "http://127.0.0.1:8888" //proxy address,optional
    });
    var app = services.BuildServiceProvider();

    var service = app.GetRequiredService<ChatGptClient>();

    var res = await service.ChatAsync(new ChatCompletionRequest
    {
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
    }, default);

    Console.WriteLine(res.Choices[0].Message.Content);
}
```
#### receive real time message
``` c#
    //To use real time message,you must pass true to request params like :
    //  ChatCompletionRequest request = new()
    //  {
    //      Stream = true,
    //      Model = "gpt-3.5-turbo",
    //      Messages = ...
    //  }
    
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
 ```
##### [More usage click here](src/Mirror.ChatGpt.Sample)

## Contributing
Contributions are welcome! If you think Mirror.ChatGPT is useful, please give me a star.
You can also vist https://xfbmx.cn for more information.

## License
Mirror.ChatGPT is released under the Apache 2.0 license. For more information, see the [LICENSE](./LICENSE) file.
