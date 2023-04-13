#if NETSTANDARD2_0
using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HttpStack;

public static class EncodingExtensions
{
    public static unsafe int GetBytes(this Encoding encoding, string s, Span<byte> bytes)
    {
        if (s.Length == 0)
            return 0;

        fixed (char* chars = s)
        fixed (byte* bytesPtr = bytes)
        {
            return encoding.GetBytes(chars, s.Length, bytesPtr, bytes.Length);
        }
    }

    public static unsafe int GetBytes(this Encoding encoding, ReadOnlySpan<char> s, Span<byte> bytes)
    {
        fixed (char* chars = s)
        fixed (byte* bytesPtr = bytes)
        {
            return encoding.GetBytes(chars, s.Length, bytesPtr, bytes.Length);
        }
    }

    public static unsafe int GetChars(this Encoding encoding, ReadOnlySpan<byte> bytes, Span<char> chars)
    {
        fixed (byte* bytesPtr = bytes)
        fixed (char* charsPtr = chars)
        {
            return encoding.GetChars(bytesPtr, bytes.Length, charsPtr, chars.Length);
        }
    }

    public static unsafe string GetString(this Encoding encoding, ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length == 0)
            return string.Empty;

        fixed (byte* bytesPtr = bytes)
        {
            return encoding.GetString(bytesPtr, bytes.Length);
        }
    }

    public static void Write(this Stream stream, ReadOnlySpan<byte> bytes)
    {
        var sharedBuffer = ArrayPool<byte>.Shared.Rent(bytes.Length);

        try
        {
            bytes.CopyTo(sharedBuffer);
            stream.Write(sharedBuffer, 0, bytes.Length);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(sharedBuffer);
        }
    }

    public static async Task<int> ReadAsync(this Stream stream, Memory<byte> bytes)
    {
        var sharedBuffer = ArrayPool<byte>.Shared.Rent(bytes.Length);

        try
        {
            var read = await stream.ReadAsync(sharedBuffer, 0, bytes.Length);

            new Span<byte>(sharedBuffer, 0, read).CopyTo(bytes.Span);

            return read;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(sharedBuffer);
        }
    }

    public static async Task WriteAsync(this Stream stream, ReadOnlyMemory<byte> bytes)
    {
        var sharedBuffer = ArrayPool<byte>.Shared.Rent(bytes.Length);

        try
        {
            bytes.Span.CopyTo(sharedBuffer);
            await stream.WriteAsync(sharedBuffer, 0, bytes.Length);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(sharedBuffer);
        }
    }
}
#endif
