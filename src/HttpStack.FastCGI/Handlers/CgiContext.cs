using System.Buffers;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using HttpStack.Collections;
using HttpStack.FastCGI.Handlers.Tcp;
using HttpStack.FastCGI.Records;

namespace HttpStack.FastCGI.Handlers;

public sealed class CgiContext : IAsyncDisposable
{
    public CgiContext()
    {
        RequestStream = new MemoryStream();
        ResponseStream = new CgiStream(this);
        RequestHeaders = new RequestHeaderDictionary();
        ResponseHeaders = new ResponseHeaderDictionary();
    }

    public SocketListener Listener { get; set; } = null!;

    public ushort Id { get; set; }

    public RequestState State { get; set; } = RequestState.None;

    public Dictionary<string, string> ServerVariables { get; } = new(StringComparer.OrdinalIgnoreCase);

    public RequestHeaderDictionary RequestHeaders { get; }

    public ResponseHeaderDictionary ResponseHeaders { get; }

    public int ResponseStatusCode { get; set; }

    public bool KeepConnection { get; set; }

    public ushort Role { get; set; }

    public MemoryStream RequestStream { get; set; }

    public CgiStream ResponseStream { get; }

    public long Start { get; set; }

    public bool DidWriteHeaders { get; set; }

    public Socket Socket { get; set; } = null!;

    public void ClearStream()
    {
        RequestStream.SetLength(0);
        RequestStream.Position = 0;
    }

    public void Reset()
    {
        Id = 0;
        State = RequestState.None;
        ServerVariables.Clear();
        KeepConnection = false;
        Role = 0;
        Socket = null!;
        Listener = null!;
        Start = 0;
        DidWriteHeaders = false;
        ResponseStatusCode = 200;
        ResponseHeaders.Clear();
        RequestHeaders.Clear();
        ResponseStream.Reset();
        ClearStream();
    }

    public async ValueTask DisposeAsync()
    {
        if (State is not RequestState.End)
        {
            await EndAsync(Id, KeepConnection);
        }

        SocketListener.Return(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static FrameHeader CreateHeader(ushort requestId, byte type, int currentLength)
    {
        return new FrameHeader(
            version: 1,
            type: type,
            requestId: requestId,
            contentLength: (ushort)currentLength,
            paddingLength: 0,
            reserved: 0);
    }

    public async ValueTask WriteAsync(byte type, ReadOnlyMemory<byte> data)
    {
        var isMultipleFrames = data.Length > Constants.MaxContentLength;
        using var owner = MemoryPool<byte>.Shared.Rent(FrameHeader.Length + (isMultipleFrames ? Constants.MaxFrameSize : data.Length));
        var memory = owner.Memory;

        while (data.Length > 0)
        {
            var currentLength = Math.Min(data.Length, Constants.MaxContentLength);
            var header = CreateHeader(Id, type, currentLength);

            header.Write(memory.Span);
            data.Span.Slice(0, currentLength).CopyTo(memory.Span.Slice(FrameHeader.Length));

            await SendAsync(memory.Slice(0, FrameHeader.Length + data.Length));
            data = data.Slice(currentLength);
        }
    }

    private async Task SendAsync(ReadOnlyMemory<byte> memory)
    {
        var pool = SocketSenderPool.Default;
        var sender = pool.Rent();

        try
        {
            await sender.SendAsync(Socket, memory);
        }
        finally
        {
            pool.Return(sender);
        }
    }

    public async ValueTask WriteEmptyFrameAsync(ushort requestId, byte type)
    {
        var header = CreateHeader(requestId, type, 0);
        using var owner = MemoryPool<byte>.Shared.Rent(FrameHeader.Length);

        header.Write(owner.Memory.Span);
        await SendAsync(owner.Memory.Slice(0, FrameHeader.Length));
    }

    private static readonly byte[] SuccessBytes =
    {
        0, 0, 0, 0, // App status
        Constants.ProtocolStatus.RequestComplete, // Protocol status
        0, 0, 0 // Reserved
    };

    public async ValueTask WriteCompleteRequestAsync(int appStatus = 0)
    {
        if (appStatus == 0)
        {
            await WriteAsync(Constants.Types.EndRequest, SuccessBytes);
            return;
        }

        using var owner = MemoryPool<byte>.Shared.Rent(8);
        BinaryPrimitives.WriteInt32BigEndian(owner.Memory.Span, appStatus);
        BinaryPrimitives.WriteInt32BigEndian(owner.Memory.Span.Slice(4), 0);
        await WriteAsync(Constants.Types.EndRequest, owner.Memory.Slice(0, 8));
    }

    public async ValueTask EndAsync(ushort id, bool keepConnection)
    {
        await WriteEmptyFrameAsync(id, Constants.Types.Stdout);
        await WriteCompleteRequestAsync(id);

        State = RequestState.End;

        if (!keepConnection)
        {
            Socket.Shutdown(SocketShutdown.Send);
        }
    }
}
