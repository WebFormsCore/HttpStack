using System;
using System.IO;
using HttpMultipartParser;
using HttpStack.Collections;
using HttpStack.Forms;

namespace HttpStack.FormParser;

public sealed class MultipartFormFile : IFormFile, IDisposable
{
    private readonly FilePart _file;
    private HeaderDictionary? _headers;
    private bool _didOpenReadStream;

    public MultipartFormFile(FilePart file)
    {
        _file = file;
    }

    public string? ContentType => _file.ContentType;

    public string? FileName => _file.FileName;

    public IHeaderDictionary Headers => _headers ??= new HeaderDictionary(_file.AdditionalProperties);

    public long Length => _file.Data.Length;

    public string? Name => _file.Name;

    public Stream OpenReadStream()
    {
        if (_didOpenReadStream)
        {
            throw new InvalidOperationException("The stream can only be opened once");
        }

        _didOpenReadStream = true;
        return _file.Data;
    }

    public void Dispose()
    {
        if (!_didOpenReadStream)
        {
            _file.Data.Dispose();
        }
    }
}
