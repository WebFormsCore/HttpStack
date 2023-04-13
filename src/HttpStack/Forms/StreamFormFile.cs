using System;
using System.IO;
using HttpStack.Collections;

namespace HttpStack.Forms;

public sealed class StreamFormFile : IFormFile, IDisposable
{
    private readonly Stream _stream;
    private bool _didOpenReadStream;

    public StreamFormFile(Stream stream, IHeaderDictionary headers, string? name, string? fileName)
    {
        _stream = stream;
        Headers = headers;
        FileName = fileName;
        Name = name;
    }

    public string? ContentType => Headers.ContentType;
    public IHeaderDictionary Headers { get; }
    public string? FileName { get; }
    public long Length => _stream.Length;
    public string? Name { get; }
    public Stream OpenReadStream()
    {
        if (_didOpenReadStream)
        {
            throw new InvalidOperationException("The stream can only be opened once");
        }

        _didOpenReadStream = true;
        return _stream;
    }

    public void Dispose()
    {
        if (!_didOpenReadStream)
        {
            _stream.Dispose();
        }
    }
}
