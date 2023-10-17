using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HttpStack.Collections;
using HttpStack.Extensions;
using Microsoft.IO;

namespace HttpStack.Forms;

internal ref struct MultipartFormParser
{
    private static readonly RecyclableMemoryStreamManager Manager = new();

    private readonly FormCollection _form;
    private readonly ReadOnlySpan<byte> _boundary;
    private MultipartParseState _state;
    private bool _didWrite;

    private HeaderDictionary _headers;
    private string? _name;
    private string? _fileName;
    private string? _localFilePath;
    private bool _removeLocalFile;

    private Stream _stream;

    public MultipartFormParser(FormCollection form, ReadOnlySpan<byte> boundary)
    {
        _boundary = boundary;
        _headers = new HeaderDictionary();
        _form = form;
        _stream = Manager.GetStream();
    }

    public bool IsFile => _fileName != null;

    public MultipartParseState State => _state;

    public ReadOnlySpan<byte> Boundary => _boundary;

    public static int GetBoundary(string header, Span<byte> buffer)
    {
        try
        {
            var boundary = ParseHeader(header.AsSpan()).boundary;

            if (boundary == null)
            {
                return 0;
            }

            var encoding = Encoding.UTF8;

            buffer[0] = (byte)'-';
            buffer[1] = (byte)'-';
            return encoding.GetBytes(boundary, buffer.Slice(2)) + 2;
        }
        catch
        {
            return 0;
        }
    }

    public void SetLocalFileAsBody(string path, bool removeFile)
    {
        _localFilePath = path;
        _removeLocalFile = removeFile;
    }

    public void Dispose()
    {
        _stream.Dispose();
    }

    public int ParseBytes(ReadOnlySpan<byte> span)
    {
        return ParseBytes(span, "\r\n"u8);
    }

    public int ParseBytes(ReadOnlySpan<byte> span, ReadOnlySpan<byte> lineEnding)
    {
        var length = 0;

        while (span.Length > 0 && _state != MultipartParseState.End)
        {
            var endIndex = span.IndexOf(lineEnding);

            if (endIndex == -1)
            {
                break;
            }

            var line = span.Slice(0, endIndex);
            span = span.Slice(endIndex + 2);
            length += endIndex + 2;
            ParseLine(line);
        }

        return length;
    }

    public void ParseLine(ReadOnlySpan<byte> line)
    {
        switch (_state)
        {
            case MultipartParseState.Boundary:
                ConsumeBoundary(line);
                break;

            case MultipartParseState.Header:
                ConsumeHeader(line);
                break;

            case MultipartParseState.Body:
                ConsumeBody(line);
                break;
        }
    }

    private void ConsumeBoundary(ReadOnlySpan<byte> line)
    {
        _state = GetBoundaryType(line) switch
        {
            BoundaryMatch.Begin => MultipartParseState.Header,
            BoundaryMatch.End => MultipartParseState.End,
            _ => MultipartParseState.Boundary
        };
    }

    private void ConsumeHeader(ReadOnlySpan<byte> line)
    {
        if (line.Length == 0)
        {
            _state = MultipartParseState.Body;
        }
        else
        {
            ParseHeader(line);
        }
    }

    private void ConsumeBody(ReadOnlySpan<byte> line)
    {
        var boundary = GetBoundaryType(line);

        if (boundary is BoundaryMatch.None)
        {
            if (line.Length == 0 && !_didWrite)
            {
                _didWrite = true;
                return;
            }

            if (_didWrite)
            {
                _stream.Write("\r\n"u8);
            }

            _stream.Write(line);
            _didWrite = true;

            return;
        }

        if (_name is not null)
        {
            if (IsFile)
            {
                AddFormFile();
            }
            else
            {
                AddFormValue();
            }
        }
        else
        {
            _stream.SetLength(0);
        }

        _didWrite = false;
        _localFilePath = null;

        _name = null;
        _fileName = null;
        _state = boundary == BoundaryMatch.End ? MultipartParseState.End : MultipartParseState.Header;
    }

    private void AddFormFile()
    {
        if (_localFilePath is not null)
        {
            _form.Files.Add(new LocalFormFile(_localFilePath, _headers, _name, _fileName, _removeLocalFile));
            _headers = new HeaderDictionary();
            _stream.SetLength(0);
        }
        else if (_stream is { Length: > 0 })
        {
            _stream.Seek(0, SeekOrigin.Begin);
            _form.Files.Add(new StreamFormFile(_stream, _headers, _name, _fileName));
            _headers = new HeaderDictionary();
            _stream = Manager.GetStream();
        }
        else
        {
            _headers.Clear();
            _stream.SetLength(0);
        }
    }

    private void AddFormValue()
    {
        var value = StreamToString();

        _stream.SetLength(0);
        _headers.Clear();

        _form.Add(_name!, value);
    }

    private string StreamToString()
    {
        string value;

        if (_stream.Length == 0)
        {
            value = string.Empty;
        }
        else if (_stream is MemoryStream memoryStream)
        {
            if (memoryStream.TryGetBuffer(out var bytes) && bytes.Array != null)
            {
                value = Encoding.UTF8.GetString(bytes.Array, bytes.Offset, bytes.Count);
            }
            else
            {
                value = Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }
        else
        {
            _stream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(_stream);
            value = reader.ReadToEnd();
        }

        return value;
    }

    private void ParseHeader(ReadOnlySpan<byte> line)
    {
        var index = line.IndexOf((byte)':');

        if (index == -1)
        {
            return;
        }

        var name = Encoding.UTF8.GetString(line.Slice(0, index).TrimEnd());
        var value = Encoding.UTF8.GetString(line.Slice(index + 1).TrimStart());

        _headers.Add(name, value);

        if (name.Equals("content-disposition", StringComparison.OrdinalIgnoreCase))
        {
            (_name, _fileName, _) = ParseHeader(value.AsSpan());
        }
    }

    public BoundaryMatch GetBoundaryType(ReadOnlySpan<byte> line)
    {
        if (!line.StartsWith(_boundary))
        {
            return BoundaryMatch.None;
        }

        var remaining = line.Slice(_boundary.Length);

        return remaining.Length switch
        {
            0 => BoundaryMatch.Begin,
            2 when remaining[0] == '-' && remaining[1] == '-' => BoundaryMatch.End,
            _ => BoundaryMatch.None
        };
    }

    private static (string? name, string? fileName, string? boundary) ParseHeader(ReadOnlySpan<char> span)
    {
        string? name = null;
        string? fileName = null;
        string? boundary = null;

        while (span.Length > 0)
        {
            var index = span.IndexOf(';');
            ReadOnlySpan<char> line;

            if (index == -1)
            {
                line = span.TrimStart();
                span = default;
            }
            else
            {
                line = span.Slice(0, index).TrimStart();
                span = span.Slice(index + 1);
            }

            var nameIndex = line.IndexOf('=');

            if (nameIndex == -1)
            {
                continue;
            }

            var nameSpan = line.Slice(0, nameIndex).TrimEnd();

            line = line.Slice(nameIndex + 1).TrimStart();

            if (line.Length == 0)
            {
                continue;
            }

            // The boundary is either a quoted string or a token.
            int endIndex;

            ReadOnlySpan<char> result;

            if (line[0] == '"')
            {
                line = line.Slice(1);

                endIndex = line.IndexOf('"');
                // TODO: Unescape the string.
                result = endIndex == -1 ? default : line.Slice(0, endIndex);
            }
            else
            {
                endIndex = line.IndexOf(';');
                result = (endIndex == -1 ? line : line.Slice(0, endIndex)).TrimEnd();
            }

            if (nameSpan.StartsWith("name".AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                if (result.Length > 0) name = result.ToString();
            }
            else if (nameSpan.StartsWith("filename".AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                fileName = result.Length > 0 ? result.ToString() : "";
            }
            else if (nameSpan.StartsWith("boundary".AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                if (result.Length > 0) boundary = result.ToString();
            }
        }

        return (name, fileName, boundary);
    }
}

public enum MultipartParseState
{
    Boundary,
    Header,
    Body,
    End
}

public enum BoundaryMatch
{
    None,
    Begin,
    End
}
