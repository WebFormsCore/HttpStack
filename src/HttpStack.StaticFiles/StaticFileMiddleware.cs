using System.Threading.Tasks;
using HttpStack.StaticFiles.Providers;
using Microsoft.Extensions.FileProviders;

namespace HttpStack.StaticFiles;

public class StaticFileMiddleware : IMiddleware
{
    private readonly IFileProvider _fileProvider;
    private readonly IContentTypeProvider _contentTypeProvider;
    private readonly PathString _prefix;

    public StaticFileMiddleware(IFileProvider fileProvider, IContentTypeProvider? contentTypeProvider = null, PathString prefix = default)
    {
        _fileProvider = fileProvider;
        _prefix = prefix;
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
        var path = _prefix + context.Request.Path;

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
