using HttpStack;
using HttpStack.AspNetCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = HttpApplication.CreateDefault();

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

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Listening on http://localhost:8080/, press any key to exit");
Console.ReadKey(true);

await server.StopAsync(CancellationToken.None);