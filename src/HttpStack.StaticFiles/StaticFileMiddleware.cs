﻿using System.Threading.Tasks;
using HttpStack.StaticFiles.Providers;
using Microsoft.Extensions.FileProviders;

namespace HttpStack.StaticFiles;

public class StaticFileMiddleware : IMiddleware
{
    private readonly IFileProvider _fileProvider;
    private readonly IContentTypeProvider _contentTypeProvider;

    public StaticFileMiddleware(IFileProvider fileProvider, IContentTypeProvider? contentTypeProvider = null)
    {
        _fileProvider = fileProvider;
        _contentTypeProvider = contentTypeProvider ?? FileExtensionContentTypeProvider.Default;
    }

    public async Task Invoke(IHttpContext context, MiddlewareDelegate next)
    {
        if (await TryServeFile(context))
        {
            return;
        }

        await next(context);
    }

    private async Task<bool> TryServeFile(IHttpContext context)
    {
        var path = context.Request.Path.Value;
        if (!_contentTypeProvider.TryGetContentType(path, out var contentType))
        {
            return false;
        }

        var fileInfo = _fileProvider.GetFileInfo(path);
        if (!fileInfo.Exists)
        {
            return false;
        }

        context.Response.StatusCode = 200;
        context.Response.ContentType = contentType;
        context.Response.ContentLength = fileInfo.Length;
#if NET
        await using var fileStream = fileInfo.CreateReadStream();
#else
        using var fileStream = fileInfo.CreateReadStream();
#endif

        await fileStream.CopyToAsync(context.Response.Body);
        return true;
    }
}
