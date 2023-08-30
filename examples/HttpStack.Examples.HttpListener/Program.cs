using System.Net;
using HttpStack;
using HttpStack.Examples.Extensions.WebSocketTime;
using HttpStack.NetHttpListener;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using var listener = new HttpListener();

listener.Prefixes.Add("http://localhost:8080/");

var builder = HttpApplication.CreateDefault();
var app = builder.Build();

app.UseTime();

app.Run(async context =>
{
    context.Response.ContentType = "text/plain";
    await context.Response.WriteAsync("Hello World!");
});

listener.Start(app);

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Listening on http://localhost:8080/, press any key to exit");
Console.ReadKey(true);

listener.Stop();
