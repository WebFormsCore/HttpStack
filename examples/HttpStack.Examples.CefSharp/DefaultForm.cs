using CefSharp;
using CefSharp.WinForms;
using HttpStack.CefSharp;
using HttpStack.Extensions;
using HttpStack.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace HttpStack.Examples.CefSharp;

public class DefaultForm : Form
{
    public DefaultForm()
    {
        Width = 800;
        Height = 600;

        var settings = new CefSettings();

        var app = new HttpStackBuilder();

        app.UseStaticFiles(new EmbeddedFileProvider(typeof(DefaultForm).Assembly), "wwwroot");

        app.Run(context => context.Response.WriteAsync($"""
            <!DOCTYPE html>
            <html>
            <body>
            <form method="post" enctype="multipart/form-data">
                value: <input type="text" name="value" text="Value">
                <input type="submit" value="Submit"><br />
                {string.Join("<br />", context.Request.Form.Select(i => $"{i.Key}: {i.Value}"))}

                <br />
                <img src="cefsharp.png" />
            </form>
            </body>
            </html>
            """));

        settings.RegisterScheme(new CefCustomScheme
        {
            SchemeName = "browser",
            SchemeHandlerFactory = app.ToSchemeHandlerFactory(),
            IsSecure = true
        });

        Cef.Initialize(settings);

        var cef = new ChromiumWebBrowser("browser://settings/");
        cef.Dock = DockStyle.Fill;
        Controls.Add(cef);
    }
}
