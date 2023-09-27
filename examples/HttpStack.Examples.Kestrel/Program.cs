using HttpStack;
using HttpStack.AspNetCore;

var builder = HttpApplication.CreateDefault();

builder.AddKestrelStack(options =>
{
    options.ListenAnyIP(8080);
});

var app = builder.Build();

app.Run(async context =>
{
    context.Response.ContentType = "text/plain";
    await context.Response.WriteAsync("Hello World!");
});

await app.RunAsync();