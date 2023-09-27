using HttpStack;
using HttpStack.AspNetCore;
using HttpStack.Examples.Extensions.WebSocketTime;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseWebSockets();

app.Use(async (context, next) =>
{
    if (!context.Request.Path.StartsWithSegments("/create"))
    {
        await next();
        return;
    }

    using var container = await context.CreateStackContextAsync();
    var response = container.Context.Response;

    response.ContentType = "text/plain";
    await response.WriteAsync("Hello World!");
});

app.UseStack(stack =>
{
    stack.UseTime();

    stack.Run(async context =>
    {
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync("Hello World!");
    });
});

app.Run();
