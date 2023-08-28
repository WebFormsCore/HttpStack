using HttpStack;
using HttpStack.Azure.Functions;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureHttpStack(app =>
    {
        app.Use((context, next) =>
        {
            context.Response.Headers["X-Powered-By"] = "HttpStack";
            return next();
        });
    })
    .Build();

host.Run();
