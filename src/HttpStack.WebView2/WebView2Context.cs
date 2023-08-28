using Microsoft.Web.WebView2.Core;

namespace HttpStack.WebView2;

public readonly struct WebView2Context
{
    public WebView2Context(
        CoreWebView2 webView2,
        CoreWebView2WebResourceRequestedEventArgs args,
        CoreWebView2Deferral deferral)
    {
        WebView2 = webView2;
        Args = args;
        Deferral = deferral;
    }

    public CoreWebView2 WebView2 { get; }

    public CoreWebView2WebResourceRequestedEventArgs Args { get; }

    public CoreWebView2Deferral Deferral { get; }
}
