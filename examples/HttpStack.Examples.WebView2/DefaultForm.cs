using HttpStack.WebView2.Extensions;
using Microsoft.Web.WebView2.Core;

namespace HttpStack.Examples.WebView2;

public class DefaultForm : Form
{
    private readonly Microsoft.Web.WebView2.WinForms.WebView2 _webView;

    public DefaultForm()
    {
        Width = 800;
        Height = 600;

        _webView = new Microsoft.Web.WebView2.WinForms.WebView2();
        _webView.Dock = DockStyle.Fill;
        Controls.Add(_webView);

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        // Note: when registering single named domains (e.g. https://httpstack), WebView2 will slow down
        //       by 2 seconds. --host-resolver-rules is a workaround for this issue.
        //       See https://github.com/MicrosoftEdge/WebView2Feedback/issues/1862 for more details.

        var environment = await CoreWebView2Environment.CreateAsync(options: new CoreWebView2EnvironmentOptions
        {
            AdditionalBrowserArguments =
                """
                --host-resolver-rules="MAP httpstack ~NOTFOUND"
                """
        });

        await _webView.EnsureCoreWebView2Async(environment);

        var app = new HttpStackBuilder();

        app.Run(async context =>
        {
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(
                $"""
                 <!DOCTYPE html>
                 <html>
                 <body>
                 <form method="post" enctype="multipart/form-data">
                     value: <input type="text" name="value" text="Value">
                     <input type="submit" value="Submit"><br />
                     {string.Join("<br />", context.Request.Form.Select(i => $"{i.Key}: {i.Value}"))}
                 </form>
                 </body>
                 </html>
                 """);
        });

        _webView.RegisterStack("https://httpstack/*", app);
        _webView.CoreWebView2.Navigate("https://httpstack/");
    }
}
