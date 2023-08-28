using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HttpStack.Host;
using Microsoft.Web.WebView2.Core;

namespace HttpStack.WebView2.Extensions;

public static class StackExtensions
{
    public static IHttpStack<WebView2Context> CreateWebView2Stack(
        this IHttpStackBuilder stackBuilder,
        IServiceProvider scopeProvider,
        MiddlewareDelegate? defaultHandler = null)
    {
        return CreateWebView2Stack(
            stackBuilder,
            new DefaultContextScopeProvider<WebView2Context>(scopeProvider),
            defaultHandler
        );
    }

    public static IHttpStack<WebView2Context> CreateWebView2Stack(
        this IHttpStackBuilder stackBuilder,
        IContextScopeProvider<WebView2Context>? scopeProvider = null,
        MiddlewareDelegate? defaultHandler = null)
    {
        return stackBuilder.CreateStack<HttpContextImpl, WebView2Context>(scopeProvider, defaultHandler);
    }

    public static void RegisterStack(this Microsoft.Web.WebView2.WinForms.WebView2 webView2, string url, IHttpStackBuilder builder)
    {
        RegisterStack(webView2.CoreWebView2, url, builder, action => webView2.Invoke(action));
    }

    public static void RegisterStack(this Microsoft.Web.WebView2.Wpf.WebView2 webView2, string url, IHttpStackBuilder builder)
    {
        RegisterStack(webView2.CoreWebView2, url, builder, webView2.Dispatcher.Invoke);
    }

    public static void RegisterStack(this CoreWebView2 webView2, string url, IHttpStackBuilder builder, Action<Action> invoke)
    {
        var stack = builder.CreateWebView2Stack();

        webView2.AddWebResourceRequestedFilter(url, CoreWebView2WebResourceContext.All);

        webView2.WebResourceRequested += (_, args) =>
        {
            var deferral = args.GetDeferral();
            var webView2Context = new WebView2Context(webView2, args, deferral);
            var context = stack.CreateContext(webView2Context);

            Task.Run(async () =>
            {
                await stack.ExecuteAsync(context);

                invoke(() =>
                {
                    var httpContext = Unsafe.As<HttpContextImpl>(context.Context);
                    httpContext.InnerContext.Args.Response = httpContext.CreateResponse();
                    httpContext.InnerContext.Deferral.Complete();
                });

                await context.DisposeAsync();
            });
        };
    }
}
