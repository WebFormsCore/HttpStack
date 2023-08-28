using HttpStack;
using HttpStack.FastCGI;

var builder = HttpApplication.CreateDefault();
var app = builder.Build();

app.Run(async context =>
{
    context.Response.ContentType = "text/plain";
    await context.Response.WriteAsync("Hello World!");
});

await app.ListenFastCgiAsync(port: 9000);
