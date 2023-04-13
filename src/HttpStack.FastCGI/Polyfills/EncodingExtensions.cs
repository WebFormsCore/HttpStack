#if NETSTANDARD2_0
using System.Buffers;
using System.Net.Sockets;
using System.Text;

namespace HttpStack.FastCGI;

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

    public static unsafe string GetString(this Encoding encoding, ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length == 0)
            return string.Empty;

        fixed (byte* bytesPtr = bytes)
        {
            return encoding.GetString(bytesPtr, bytes.Length);
        }
    }

    public static bool TryDequeue<T>(this Queue<T> queue, out T item)
    {
        if (queue.Count == 0)
        {
            item = default!;
            return false;
        }

        item = queue.Dequeue();
        return true;
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
}
#endif
