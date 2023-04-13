using HttpStack;
using HttpStack.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStack(stack =>
{
    stack.Run(async context =>
    {
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync("Hello World!");
    });
});

app.Run();
