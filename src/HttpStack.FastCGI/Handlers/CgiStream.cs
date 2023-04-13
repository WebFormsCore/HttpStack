using System.Text;

namespace HttpStack.FastCGI.Handlers;

public class CgiStream : Stream
{
    private readonly CgiContext _context;
    private readonly MemoryStream _stream = new();

    public CgiStream(CgiContext context)
    {
        _context = context;
    }

    private void WriteHeaders()
    {
        _context.DidWriteHeaders = true;

        var encoding = Encoding.ASCII;
        var length = 0;

        // Calculate the length of the headers
        var headerPrefix = "HTTP/1.1 "u8;
        length += headerPrefix.Length // HTTP/1.1
                  + 3 // Status code
                  + 2; // "\r\n"

        var statusHeaderPrefix = "Status: "u8;
        length += statusHeaderPrefix.Length // Status:
                  + 3 // Status code
                  + 2; // "\r\n"

        foreach (var kv in _context.ResponseHeaders)
        {
            length += encoding.GetByteCount(kv.Key)// Key
                      + 2 // ": "
                      + kv.Value.Count switch
                      {
                          0 => 0, // No values
                          1 => encoding.GetByteCount(kv.Value[0]), // Single value
                          _ => kv.Value.Sum(v => encoding.GetByteCount(v)) + ((kv.Value.Count - 1) * 2) // Multiple values with ", "
                      }
                      + 2; // "\r\n"
        }

        length += 4; // "\r\n\r\n"

        _stream.SetLength(length);

        // Write the HTTP version
        var span = _stream.GetBuffer().AsSpan();

        headerPrefix.CopyTo(span);

        // Write the status code
        var statusCode = _context.ResponseStatusCode;
        span[9] = (byte)(statusCode / 100 + '0');
        span[10] = (byte)(statusCode / 10 % 10 + '0');
        span[11] = (byte)(statusCode % 10 + '0');
        span[12] = (byte)'\r';
        span[13] = (byte)'\n';

        // Write the status header
        statusHeaderPrefix.CopyTo(span.Slice(14));
        span.Slice(9, 3).CopyTo(span.Slice(22));
        span[25] = (byte)'\r';
        span[26] = (byte)'\n';

        var offset = 27;

        // Write the headers
        foreach (var kv in _context.ResponseHeaders)
        {
            offset += encoding.GetBytes(kv.Key, span.Slice(offset));

            span[offset++] = (byte)':';
            span[offset++] = (byte)' ';

            for (var i = 0; i < kv.Value.Count; i++)
            {
                var v = kv.Value[i];

                if (i != 0)
                {
                    span[offset++] = (byte)',';
                    span[offset++] = (byte)' ';
                }

                offset += encoding.GetBytes(v, span.Slice(offset));
            }

            span[offset++] = (byte)'\r';
            span[offset++] = (byte)'\n';
        }

        span[offset++] = (byte)'\r';
        span[offset++] = (byte)'\n';
        span[offset++] = (byte)'\r';
        span[offset] = (byte)'\n';

        _stream.Position = length;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException("This stream does not support reading.");
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException("This stream does not support seeking.");
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException("This stream does not support setting length.");
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (!_context.DidWriteHeaders)
        {
            WriteHeaders();
        }

        _stream.Write(buffer, offset, count);
    }

    public override void Flush()
    {
        throw new NotSupportedException("Only async flush is supported.");
    }

    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        if (_stream.Length == 0)
        {
            return;
        }

        if (_stream.TryGetBuffer(out var segment))
        {
            await _context.WriteAsync(Constants.Types.Stdout, segment);
        }
        else
        {
            var data = _stream.GetBuffer();
            await _context.WriteAsync(Constants.Types.Stdout, data.AsMemory(0, (int)_stream.Length));
        }

        _stream.SetLength(0);
    }

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public void Reset()
    {
        _stream.SetLength(0);
        _stream.Position = 0;
    }
}
