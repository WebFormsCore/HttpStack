using System;
using System.Threading;
using System.Threading.Tasks;

namespace HttpStack;

public interface IStackService : IDisposable
{
    Task StartAsync(IHttpStackBuilder builder, CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);
}
