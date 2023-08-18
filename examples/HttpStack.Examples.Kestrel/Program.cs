using HttpStack;
using HttpStack.AspNetCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = new HttpApplicationBuilder();

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.ListenAnyIP(8080);
});

var app = builder.Build();

app.Run(async context =>
{
    context.Response.ContentType = "text/plain";
    await context.Response.WriteAsync("Hello World!");
});

using var server = await app.ListenKestrelAsync();

Console.WriteLine("Listening on http://localhost:8080/");
Console.WriteLine("Press any key to exit");
Console.ReadKey(true);

await server.StopAsync(CancellationToken.None);