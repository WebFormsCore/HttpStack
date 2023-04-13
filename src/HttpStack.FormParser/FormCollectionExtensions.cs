using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HttpMultipartParser;
using HttpStack.Collections;

namespace HttpStack.FormParser;

public static class FormCollectionExtensions
{
    public static async Task LoadAsync(this FormCollection collection, IHttpRequest httpRequest)
    {
        if (httpRequest is not { Headers.ContentType: { } contentType, Method: not ("GET" or "OPTIONS") })
        {
            return;
        }

        if (contentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
        {
            await InitializeUrlEncodedForm(collection, httpRequest);
        }
        else if (contentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
        {
            await InitializeFormData(collection, httpRequest);
        }
    }

    private static async Task InitializeFormData(FormCollection collection, IHttpRequest httpRequest)
    {
        var parser = await MultipartFormDataParser.ParseAsync(httpRequest.Body);

        foreach (var param in parser.Parameters)
        {
            collection.Add(param.Name, param.Data);
        }

        foreach (var file in parser.Files)
        {
            collection.Files.Add(new MultipartFormFile(file));
        }
    }

    private static async Task InitializeUrlEncodedForm(FormCollection collection, IHttpRequest httpRequest)
    {
        using var reader = new StreamReader(httpRequest.Body, Encoding.UTF8);
        var form = await reader.ReadToEndAsync();
        var current = HttpUtility.ParseQueryString(form);

        foreach (var key in current.AllKeys)
        {
            collection.Add(key!, current[key]!);
        }
    }
}
