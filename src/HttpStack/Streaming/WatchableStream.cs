using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HttpStack.Streaming;

public class WatchableStream : Stream
{
    private Stream _stream = null!;

    public bool DidWrite { get; private set; }

    public void SetStream(Stream inner)
    {
        _stream = inner;

        if (!inner.CanSeek) return;

        try
        {
            DidWrite = inner.Position > 0;
            return;
        }
        catch
        {
            // ignore
        }

        try
        {
            DidWrite = _stream.Length > 0;
        }
        catch
        {
            // ignore
        }
    }

    public void Reset()
    {
        _stream = null!;
        DidWrite = false;
    }

    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
    {
        return _stream.BeginRead(buffer, offset, count, callback, state);
    }

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
    {
        DidWrite = true;
        return _stream.BeginWrite(buffer, offset, count, callback, state);
    }

    public override void Close()
    {
        _stream.Close();
    }

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        return _stream.CopyToAsync(destination, bufferSize, cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _stream.Dispose();
        }
    }

    public override int EndRead(IAsyncResult asyncResult)
    {
        return _stream.EndRead(asyncResult);
    }

    public override void EndWrite(IAsyncResult asyncResult)
    {
        _stream.EndWrite(asyncResult);
    }

    public override void Flush()
    {
        DidWrite = true;
        _stream.Flush();
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        DidWrite = true;
        return _stream.FlushAsync(cancellationToken);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return _stream.Read(buffer, offset, count);
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return _stream.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override int ReadByte()
    {
        return _stream.ReadByte();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _stream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _stream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        DidWrite = true;
        _stream.Write(buffer, offset, count);
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        DidWrite = true;
        return _stream.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override void WriteByte(byte value)
    {
        DidWrite = true;
        _stream.WriteByte(value);
    }

    public override bool CanRead => _stream.CanRead;
    public override bool CanSeek => _stream.CanSeek;
    public override bool CanTimeout => _stream.CanTimeout;
    public override bool CanWrite => _stream.CanRead;
    public override long Length => _stream.Length;

    public override long Position
    {
        get => _stream.Position;
        set
        {
            DidWrite = true;
            _stream.Position = value;
        }
    }

    public override int ReadTimeout
    {
        get => _stream.ReadTimeout;
        set => _stream.ReadTimeout = value;
    }

    public override int WriteTimeout
    {
        get => _stream.WriteTimeout;
        set => _stream.WriteTimeout = value;
    }

    public override bool Equals(object? obj)
    {
        return _stream.Equals(obj);
    }

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return _stream.GetHashCode();
    }

    public override string? ToString()
    {
        return _stream.ToString();
    }
}
