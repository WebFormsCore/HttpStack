using HttpStack;
using HttpStack.Examples.Extensions.WebSocketTime;
using HttpStack.NetHttpListener;
using Microsoft.Extensions.Hosting;

var builder = HttpApplication.CreateDefault();

builder.AddHttpListener("http://localhost:8080/");

var app = builder.Build();

app.UseTime();

app.Run(async context =>
{
    context.Response.ContentType = "text/plain";
    await context.Response.WriteAsync("Hello World!");
});

await app.RunAsync();