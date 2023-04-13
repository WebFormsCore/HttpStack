using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using HttpStack.FastCGI.Handlers;
using HttpStack.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;

namespace HttpStack.FastCGI;

public static class StackExtensions
{
#if NET6_0_OR_GREATER
    public static async Task ListenFastCgiAsync(this IHttpStackBuilder app, string unixSocketPath, CancellationToken token = default)
    {
        var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

        socket.Bind(new UnixDomainSocketEndPoint(unixSocketPath));
        socket.Listen(100);

        await app.ListenFastCgiAsync(socket, token);
    }
#endif

    public static async Task ListenFastCgiAsync(this IHttpStackBuilder app, int port, CancellationToken token = default)
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(new IPEndPoint(IPAddress.Loopback, port));
        socket.Listen(100);

        await app.ListenFastCgiAsync(socket, token);
    }

    public static async Task ListenFastCgiAsync(this IHttpStackBuilder app, Socket socket, CancellationToken token = default)
    {
        using var socketListenerPool = new SocketListenerPool(64);
        var stack = app.CreateStack<HttpContextImpl, CgiContext>();
        var channel = Channel.CreateUnbounded<CgiContext>();
        var processorCount = Environment.ProcessorCount;
        var logger = app.Services.GetService<ILogger<SocketListener>>();

        for (var i = 0; i < processorCount; i++)
        {
            _ = Task.Run(async () =>
            {
                while (await channel.Reader.WaitToReadAsync(token))
                {
                    while (channel.Reader.TryRead(out var request))
                    {
                        try
                        {
                            await stack.ProcessRequestAsync(request);
                            await request.ResponseStream.FlushAsync(token);
                            await request.DisposeAsync();
                        }
                        catch(Exception e)
                        {
                            if (logger is not null)
                            {
                                logger.LogError(e, "Error while processing request");
                            }
                            else
                            {
                                Console.WriteLine("Error while processing request: {0}", e);
                            }
                        }
                    }
                }
            }, token);
        }

        while (!token.IsCancellationRequested)
        {
            var listener = socketListenerPool.Get();
            var connection =
#if NET
                await socket.AcceptAsync(listener.Socket, token);
#else
                await socket.AcceptAsync();
#endif

            _ = Task.Run(async () =>
            {
                try
                {
                    await listener.ListenAsync(connection, channel.Writer);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Error while listening: {0}", e);
                }
                finally
                {
                    // ReSharper disable once AccessToDisposedClosure
                    socketListenerPool.Return(listener);
                }
            }, token);
        }

        socket.Dispose();
    }
}
