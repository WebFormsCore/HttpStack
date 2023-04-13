using System;
using System.IO;
using System.Threading.Tasks;
using CefSharp;
using HttpStack.Host;

namespace HttpStack.CefSharp;

internal class HttpStackHandlerFactory : ISchemeHandlerFactory
{
    private readonly IHttpStack<CefContext> _stack;

    public HttpStackHandlerFactory(IHttpStack<CefContext> serviceProvider)
    {
        _stack = serviceProvider;
    }

    public IResourceHandler Create(IBrowser browser, IFrame frame, string schemeName, IRequest request)
    {
        var stream = new MemoryStream();
        return new AsyncResourceHandler(_stack, stream: stream, autoDisposeStream: true);
    }

    private class AsyncResourceHandler : ResourceHandler
    {
        private readonly IHttpStack<CefContext> _stack;

        public AsyncResourceHandler(
            IHttpStack<CefContext> stack,
            string mimeType = "text/html",
            Stream? stream = null,
            bool autoDisposeStream = false,
            string? charset = null)
            : base(mimeType, stream, autoDisposeStream, charset)
        {
            _stack = stack;
        }

        public override CefReturnValue ProcessRequestAsync(IRequest request, ICallback callback)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var context = new CefContext(request, this);
                    await _stack.ProcessRequestAsync(context);
                    callback.Continue();
                }
                catch
                {
                    callback.Cancel();
                }
                finally
                {
                    callback.Dispose();
                }
            });

            return CefReturnValue.ContinueAsync;
        }

        public override void Dispose()
        {
            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
