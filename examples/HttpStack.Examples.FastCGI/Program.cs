using HttpStack;
using HttpStack.FastCGI;

var stack = new HttpApplicationBuilder();

var app = stack.Build();

app.Run(async context =>
{
    context.Response.ContentType = "text/plain";
    await context.Response.WriteAsync("Hello World!");
});

await app.ListenFastCgiAsync(port: 9000);
