using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace HttpStack;

public class HostEnvironment : IHostEnvironment
{
    private IFileProvider? _contentRootFileProvider;
    public string EnvironmentName { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string ContentRootPath { get; set; } = string.Empty;

    public IFileProvider ContentRootFileProvider
    {
        get => _contentRootFileProvider ??= new NullFileProvider();
        set => _contentRootFileProvider = value;
    }
}
