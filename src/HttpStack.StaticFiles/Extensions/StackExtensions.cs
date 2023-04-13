﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace HttpStack.StaticFiles;

public static class StackExtensions
{
    public static IHttpStackBuilder UseStaticFiles(this IHttpStackBuilder builder, IFileProvider fileProvider)
    {
        builder.UseMiddleware(ActivatorUtilities.CreateInstance<StaticFileMiddleware>(builder.Services, fileProvider));
        return builder;
    }
}
