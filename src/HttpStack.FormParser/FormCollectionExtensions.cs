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
    public static Task LoadAsync(this FormCollection collection, IReadOnlyHttpRequest request)
    {
        return LoadAsync(collection, request.Method, request.ContentType, request.Body);
    }

    public static async Task LoadAsync(this FormCollection collection, string method, string? contentType, Stream stream)
    {
        if (method is not ("GET" or "OPTIONS") || contentType is null)
        {
            return;
        }

        if (contentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
        {
            await InitializeUrlEncodedForm(collection, stream);
        }
        else if (contentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
        {
            await InitializeFormData(collection, stream);
        }
    }

    private static async Task InitializeFormData(FormCollection collection, Stream stream)
    {
        var parser = await MultipartFormDataParser.ParseAsync(stream);

        foreach (var param in parser.Parameters)
        {
            collection.Add(param.Name, param.Data);
        }

        foreach (var file in parser.Files)
        {
            collection.Files.Add(new MultipartFormFile(file));
        }
    }

    private static async Task InitializeUrlEncodedForm(FormCollection collection, Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var form = await reader.ReadToEndAsync();
        var current = HttpUtility.ParseQueryString(form);

        foreach (var key in current.AllKeys)
        {
            collection.Add(key!, current[key]!);
        }
    }
}
