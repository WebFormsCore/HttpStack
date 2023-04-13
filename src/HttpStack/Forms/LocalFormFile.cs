using System;
using System.IO;
using HttpStack.Collections;

namespace HttpStack.Forms;

public sealed class LocalFormFile : IFormFile, IDisposable
{
    private readonly string _filePath;
    private readonly bool _removeLocalFile;
    private long? _length;

    public LocalFormFile(
        string filePath,
        IHeaderDictionary headers,
        string? name,
        string? fileName,
        bool removeLocalFile = false)
    {
        _filePath = filePath;
        _removeLocalFile = removeLocalFile;
        Headers = headers;
        FileName = fileName;
        Name = name;
    }

    public string? ContentType => Headers.ContentType;
    public IHeaderDictionary Headers { get; }
    public string? FileName { get; }
    public long Length => _length ??= new FileInfo(_filePath).Length;
    public string? Name { get; }
    public Stream OpenReadStream() => File.Open(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

    public void Dispose()
    {
        if (_removeLocalFile)
        {
            try
            {
                File.Delete(_filePath);
            }
            catch
            {
                // ignored
            }
        }
    }
}
