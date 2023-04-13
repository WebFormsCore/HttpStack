using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Channels;
using Ben.Collections.Specialized;
using HttpStack.FastCGI.Handlers.Tcp;
using HttpStack.FastCGI.Records;
using Microsoft.Extensions.ObjectPool;
using ValueTaskSupplement;

namespace HttpStack.FastCGI.Handlers;

public sealed class SocketListener : IDisposable
{
    private static int _closedCount;
    private static int _requestCount;
    private static readonly ObjectPool<CgiContext> RequestPool = new DefaultObjectPool<CgiContext>(new RequestPooledObjectPolicy(), 512);
    private readonly Dictionary<ushort, CgiContext> _requests = new();
    private readonly InternPool _serverKeyPool = new(capacity: 256, maxCount: 256, maxLength: 4096);
    private readonly InternPool _headerKeyPool = new(capacity: 256, maxCount: 256, maxLength: 128);
    private readonly Pipe _pipe = new();
    private readonly SocketReceiver _receiver = new();
    private readonly Queue<CgiContext> _requestQueue = new();
    private ChannelWriter<CgiContext>? _channelWriter;
    private Socket? _socket;

    public Socket? Socket => _socket;

    public async Task ListenAsync(Socket socket, ChannelWriter<CgiContext> writer)
    {
        if (Interlocked.CompareExchange(ref _channelWriter, writer, null) != null)
        {
            throw new InvalidOperationException("The listener can only be used by one socket at a time.");
        }

        _socket = socket;

        var writing = FillPipeAsync(_pipe.Writer, socket);
        var reading = ReadPipeAsync(_pipe.Reader);

        await ValueTaskEx.WhenAll(reading, writing);

        _pipe.Reset();
        _channelWriter = null;

        socket.Shutdown(SocketShutdown.Send);

#if NET6_0_OR_GREATER
        await socket.DisconnectAsync(reuseSocket: true);
#else
        await Task.Factory.FromAsync(socket.BeginDisconnect, socket.EndDisconnect, true, null);
#endif
    }

    private async ValueTask FillPipeAsync(PipeWriter writer, Socket socket)
    {
        const int minimumBufferSize = 512;
        var requests = 0;

        while (socket.Connected)
        {
            var requestCounter = Interlocked.Increment(ref _requestCount);
            var memory = writer.GetMemory(minimumBufferSize);
            var result = await _receiver.ReceiveAsync(socket, memory);

            if (result.HasError)
            {
                Console.WriteLine(result.SocketError?.Message);
                break;
            }

            if (result.BytesTransferred == 0)
            {
                Console.WriteLine($"Connection closed (requests: {requests} of {requestCounter}, disconnects: {Interlocked.Increment(ref _closedCount)})");
                break;
            }

            requests++;

            writer.Advance(result.BytesTransferred);

            var flushResult = await writer.FlushAsync();

            if (flushResult.IsCompleted)
            {
                break;
            }
        }

        await writer.CompleteAsync();
    }

    private async ValueTask ReadPipeAsync(PipeReader reader)
    {
        while (true)
        {
            var result = await reader.ReadAsync();
            var buffer = result.Buffer;

            while (buffer.Length >= FrameHeader.Length)
            {
                if (!FrameHeader.Parse(ref buffer, out var header))
                {
                    break;
                }

                var length = FrameHeader.Length + header.ContentLength + header.PaddingLength;

                if (buffer.Length < length)
                {
                    break;
                }

                var data = buffer.Slice(FrameHeader.Length, header.ContentLength);
                ProcessFrame(ref header, ref data);

                buffer = buffer.Slice(length);
            }

            reader.AdvanceTo(buffer.Start, buffer.End);

            if (_channelWriter is not { } writer)
            {
                throw new InvalidOperationException("The channel writer is not set.");
            }

            while (_requestQueue.TryDequeue(out var request))
            {
                await writer.WriteAsync(request);
            }

            if (result.IsCompleted)
            {
                break;
            }
        }

        await reader.CompleteAsync();
    }

    private void ProcessFrame(ref FrameHeader header, ref ReadOnlySequence<byte> contentAndPadding)
    {
        var reader = new SequenceReader<byte>(contentAndPadding);

        switch (header.Type)
        {
            case Constants.Types.BeginRequest:
                BeginRequest(header.RequestId, ref reader);
                break;
            case Constants.Types.EndRequest:
            case Constants.Types.AbortRequest:
                EndRequest(header.RequestId);
                break;
            case Constants.Types.Params:
                AddParams(header.RequestId, ref reader);
                break;
            case Constants.Types.Stdin:
                AddStdin(header.RequestId, ref reader);
                break;
        }
    }

    private void BeginRequest(ushort requestId, ref SequenceReader<byte> reader)
    {
        CgiContext cgiContext;

        do
        {
            cgiContext = RequestPool.Get();
        } while (cgiContext.State is not RequestState.None);

        var role = reader.ReadUInt16BigEndian();
        var flags = reader.ReadByte();
        var keepConnection = (flags & Constants.Flags.KeepConnection) != 0;

        cgiContext.Start = Stopwatch.GetTimestamp();
        cgiContext.Id = requestId;
        cgiContext.KeepConnection = keepConnection;
        cgiContext.Role = role;
        cgiContext.Socket = _socket!;
        cgiContext.Listener = this;
        cgiContext.State = RequestState.Headers;

        _requests[requestId] = cgiContext;
    }

    private void EndRequest(ushort requestId)
    {
        _requests.Remove(requestId);
    }

    private void AddParams(ushort requestId, ref SequenceReader<byte> currentReader)
    {
        if (!_requests.TryGetValue(requestId, out var request))
        {
            return;
        }

        if (currentReader.Length == 0)
        {
            request.State = RequestState.RequestBody;
            request.ClearStream();
            return;
        }

        Debug.Assert(request.State is RequestState.Headers);

        SequenceReader<byte> reader;
        var isFromStream = false;

        // Check if there are pending bytes from the previous frame
        if (request.RequestStream.Length > 0)
        {
            // Move the current reader to the pending stream
            var stream = request.RequestStream;

            foreach (var seq in currentReader.UnreadSequence)
            {
                stream.Write(seq.Span);
            }

            // Create a new reader from the pending stream
            var memory = stream.GetBuffer().AsMemory(0, (int)stream.Length);
            var sequence = new ReadOnlySequence<byte>(memory);

            reader = new SequenceReader<byte>(sequence);
            isFromStream = true;
        }
        else
        {
            reader = currentReader;
        }

        while (reader.Remaining > 0)
        {
            // Try to read the length of the name and value
            if (!reader.TryReadVarLength(out var nameLength, out var length))
            {
                break;
            }

            var readLength = length;

            if (!reader.TryReadVarLength(out var valueLength, out length))
            {
                reader.Rewind(readLength);
                break;
            }

            readLength += length;

            // Try to read the name and value
            var contentLength = nameLength + valueLength;

            if (reader.Remaining < contentLength)
            {
                // We don't have enough data to read the name and value, so we need to store the current reader
                reader.Rewind(readLength);

                foreach (var seq in reader.UnreadSequence)
                {
                     request.RequestStream.Write(seq.Span);
                }

                reader.Advance(reader.Remaining);
                break;
            }

            var (type, name) = reader.ReadStringOrHeader((int)nameLength, _serverKeyPool, _headerKeyPool);
            var value = reader.ReadString((int)valueLength);

            if (type == StringType.Header)
            {
                request.RequestHeaders[name] = value;
            }
            else
            {
                request.ServerVariables[name] = value;
            }
        }

        if (isFromStream)
        {
            // Set the stream length to the remaining data
            var stream = request.RequestStream;
            var buffer = stream.GetBuffer();

            buffer.AsSpan().Slice((int)reader.Consumed, (int)reader.Remaining).CopyTo(buffer);
            stream.SetLength(reader.Remaining);
        }
    }

    private void AddStdin(ushort requestId, ref SequenceReader<byte> reader)
    {
        if (!_requests.TryGetValue(requestId, out var request))
        {
            return;
        }

        if (reader.Length == 0)
        {
            request.RequestStream.Position = 0;
            request.State = RequestState.ResponseBody;

            if (_channelWriter is not {} writer || !writer.TryWrite(request))
            {
                _requestQueue.Enqueue(request);
            }
            return;
        }

        Debug.Assert(request.State is RequestState.RequestBody);

        foreach (var seq in reader.UnreadSequence)
        {
            request.RequestStream.Write(seq.Span);
        }
    }

    public void Reset()
    {
        _requests.Clear();
        _requestQueue.Clear();
    }

    public static void Return(CgiContext cgiContext) => RequestPool.Return(cgiContext);

    public void Dispose()
    {
        _socket?.Dispose();
        _socket = null;
    }
}
