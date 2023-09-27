using System.Collections.Generic;
using HttpStack.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HttpStack;

public interface IHttpApplicationBuilder : IHttpHostBuilder
{
    ILoggingBuilder Logging { get; }

    IHostEnvironment Environment { get; }

    IDictionary<object, object> Properties { get; }

    IHttpStackBuilder Build();
}