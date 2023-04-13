// See https://aka.ms/new-console-template for more information

using System.Net;
using HttpStack;
using HttpStack.NetHttpListener;

using var listener = new HttpListener();

listener.Prefixes.Add("http://localhost:8080/");

var app = new HttpStackBuilder();

app.Run(async context =>
{
    context.Response.ContentType = "text/plain";
    await context.Response.WriteAsync("Hello World!");
});

listener.Start(app);

Console.WriteLine("Listening on http://localhost:8080/");
Console.WriteLine("Press any key to exit");
Console.ReadKey(true);

listener.Stop();
