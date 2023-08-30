using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpStack.Examples.Extensions.WebSocketTime;

public static class StackExtensions
{
    public static void UseTime(this IHttpStackBuilder stack)
    {
        stack.RunPath("/ws", context =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                return Task.CompletedTask;
            }

            context.WebSockets.AcceptWebSocketRequest(async (httpContext, socket) =>
            {
                var cts = new CancellationTokenSource();

                _ = Task.Run(async () =>
                {
                    var receiving = new ArraySegment<byte>(new byte[1024]);

                    while (true)
                    {
                        try
                        {
                            var result = await socket.ReceiveAsync(receiving, default);

                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, default);
                                break;
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                    }

                    cts.Cancel();
                });

                const string dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
                var sending = new byte[dateTimeFormat.Length];

                while (!cts.IsCancellationRequested)
                {
                    var now = DateTime.Now.ToString(dateTimeFormat);
                    var length = Encoding.UTF8.GetBytes(now, sending);
                    var bytes = new ArraySegment<byte>(sending, 0, length);

                    await socket.SendAsync(bytes, WebSocketMessageType.Text, true, default);

                    await Task.Delay(TimeSpan.FromSeconds(1), cts.Token);
                }
            });

            return Task.CompletedTask;
        });

        stack.RunPath("/time", async context =>
        {
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(
                """
                <!DOCTYPE html>
                <html>
                <body>
                    <div id="time"></div>
                    <script>
                        const time = document.getElementById('time');
                        const socket = new WebSocket(`ws://${location.host}/ws`);
                        
                        socket.onmessage = function (event) {
                            time.innerText = event.data;
                        };
                    </script>
                </body>
                </html>
                """);
        });
    }
}
