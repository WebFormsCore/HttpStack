using System.Buffers;
using System.Buffers.Binary;

namespace HttpStack.FastCGI.Records;

public readonly struct FrameHeader
{
    public const int Length = 1 // Version
        + 1 // Type
        + 2 // RequestId
        + 2 // ContentLength
        + 1 // PaddingLength
        + 1; // Reserved

    public FrameHeader(byte version, byte type, ushort requestId, ushort contentLength, byte paddingLength, byte reserved)
    {
        Version = version;
        Type = type;
        RequestId = requestId;
        ContentLength = contentLength;
        PaddingLength = paddingLength;
        Reserved = reserved;
    }

    public readonly byte Version;
    public readonly byte Type;
    public readonly ushort RequestId;
    public readonly ushort ContentLength;
    public readonly byte PaddingLength;
    public readonly byte Reserved;

    public static bool Parse(ref ReadOnlySequence<byte> sequence, out FrameHeader header)
    {
        var reader = new SequenceReader<byte>(sequence);
        Span<byte> span = stackalloc byte[Length];

        if (!reader.TryCopyTo(span))
        {
            header = default;
            return false;
        }

        header = new FrameHeader(
            span[0],
            span[1],
            BinaryPrimitives.ReadUInt16BigEndian(span.Slice(2)),
            BinaryPrimitives.ReadUInt16BigEndian(span.Slice(4)),
            span[6],
            span[7]);

        return true;
    }

    public void Write(Span<byte> span)
    {
        span[0] = Version;
        span[1] = Type;
        BinaryPrimitives.WriteUInt16BigEndian(span.Slice(2, 2), RequestId);
        BinaryPrimitives.WriteUInt16BigEndian(span.Slice(4, 2), ContentLength);
        span[6] = PaddingLength;
        span[7] = Reserved;
    }
}
