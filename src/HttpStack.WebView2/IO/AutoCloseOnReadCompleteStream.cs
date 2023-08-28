using System.IO;
using Microsoft.Extensions.ObjectPool;

namespace HttpStack.WebView2.IO;

internal class AutoCloseOnReadCompleteStream : Stream
{
    public static readonly ObjectPool<AutoCloseOnReadCompleteStream> Pool = new DefaultObjectPool<AutoCloseOnReadCompleteStream>(new DefaultPooledObjectPolicy<AutoCloseOnReadCompleteStream>());

    private Stream _baseStream = null!;

    public void Initialize(Stream baseStream)
    {
        _baseStream = baseStream;
    }

    public override bool CanRead => _baseStream.CanRead;

    public override bool CanSeek => _baseStream.CanSeek;

    public override bool CanWrite => _baseStream.CanWrite;

    public override long Length => _baseStream.Length;

    public override long Position { get => _baseStream.Position; set => _baseStream.Position = value; }

    public override void Flush() => _baseStream?.Flush();

    public override int Read(byte[] buffer, int offset, int count)
    {
        var bytesRead = _baseStream.Read(buffer, offset, count);

        if (bytesRead == 0)
        {
            _baseStream.Close();
            _baseStream = null!;
            Pool.Return(this);
        }

        return bytesRead;
    }

    public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);

    public override void SetLength(long value) => _baseStream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => _baseStream?.Write(buffer, offset, count);
}