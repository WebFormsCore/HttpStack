using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;
using Ben.Collections.Specialized;

namespace HttpStack.FastCGI;

internal static class ReaderExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ReadUInt16BigEndian(this ref SequenceReader<byte> reader)
    {
        Span<byte> span = stackalloc byte[2];
        if (!reader.TryCopyTo(span)) throw new InvalidOperationException();
        reader.Advance(2);
        return BinaryPrimitives.ReadUInt16BigEndian(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ReadByte(this ref SequenceReader<byte> reader)
    {
        if (!reader.TryRead(out var value)) throw new InvalidOperationException();
        return value;
    }

    public static bool TryReadVarLength(this ref SequenceReader<byte> reader, out uint value, out int length)
    {
        if (!reader.TryRead(out var firstByte))
        {
            value = 0;
            length = 0;
            return false;
        }

        if (firstByte <= 127)
        {
            value = firstByte;
            length = 1;
            return true;
        }

        Span<byte> span = stackalloc byte[3];

        if (!reader.TryCopyTo(span))
        {
            value = 0;
            length = 0;
            return false;
        }

        reader.Advance(3);

        value = (uint)(0x1000000 * (0x7F & firstByte) + 0x10000 * span[0] + 0x100 * span[1] + span[2]);
        length = 4;
        return true;
    }

    public static string ReadString(this ref SequenceReader<byte> reader, int length, InternPool? pool = null)
    {
        return reader.ReadString(length, true, pool, null).Value;
    }

    public static (StringType Type, string Value) ReadStringOrHeader(this ref SequenceReader<byte> reader, int length, InternPool serverPool, InternPool headerPool)
    {
        return reader.ReadString(length, true, serverPool, headerPool);
    }

    private static (StringType Type, string Value) ReadString(this ref SequenceReader<byte> reader, int length, bool allowHeader, InternPool? serverPool, InternPool? headerPool)
    {
        if (length > 1024)
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent(length);
            var memory = memoryOwner.Memory.Span.Slice(0, length);

            if (!reader.TryCopyTo(memory)) throw new InvalidOperationException();
            reader.Advance(length);

            return CreateString(memory, allowHeader, serverPool, headerPool);
        }

        Span<byte> span = stackalloc byte[length];

        if (!reader.TryCopyTo(span)) throw new InvalidOperationException();
        reader.Advance(length);

        return CreateString(span, allowHeader, serverPool, headerPool);
    }

    private static (StringType Type, string Value) CreateString(Span<byte> span, bool allowHeader, InternPool? serverPool, InternPool? headerPool)
    {
        if (!allowHeader || span.Length < 6 || !span.Slice(0, 5).SequenceEqual("HTTP_"u8))
        {
            return (StringType.Default, serverPool is null ? Encoding.UTF8.GetString(span) : serverPool.InternUtf8(span));
        }

        var remaining = span.Slice(5);

        // Replace _ with - in header names
        var index = remaining.IndexOf((byte)'_');

        while (index != -1)
        {
            remaining[index] = (byte)'-';
            index = remaining.IndexOf((byte)'_');
        }

        return (
            StringType.Header,
            headerPool is null || remaining.Length > 40
                ? Encoding.UTF8.GetString(remaining)
                : headerPool.InternUtf8(remaining)
        );
    }
}
