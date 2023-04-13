// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO.Pipelines;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace HttpStack.FastCGI.Handlers.Tcp;

internal sealed class SocketReceiver : SocketAwaitableEventArgs
{
    public SocketReceiver() : this(PipeScheduler.ThreadPool)
    {
    }

    public SocketReceiver(PipeScheduler ioScheduler) : base(ioScheduler)
    {
    }

    public ValueTask<SocketOperationResult> WaitForDataAsync(Socket socket)
    {
#if NET
        SetBuffer(Memory<byte>.Empty);
#else
        SetBuffer(Array.Empty<byte>(), 0, 0);
#endif

        if (socket.ReceiveAsync(this))
        {
            return new ValueTask<SocketOperationResult>(this, 0);
        }

        var bytesTransferred = BytesTransferred;
        var error = SocketError;

        return error == SocketError.Success
            ? new ValueTask<SocketOperationResult>(new SocketOperationResult(bytesTransferred))
            : new ValueTask<SocketOperationResult>(new SocketOperationResult(CreateException(error)));
    }

    public ValueTask<SocketOperationResult> ReceiveAsync(Socket socket, Memory<byte> buffer)
    {
#if NET
        SetBuffer(buffer);
#else
        SetBuffer(MemoryMarshal.TryGetArray((ReadOnlyMemory<byte>)buffer, out var segment) ? segment.Array : throw new InvalidOperationException(), 0, buffer.Length);
#endif

        if (socket.ReceiveAsync(this))
        {
            return new ValueTask<SocketOperationResult>(this, 0);
        }

        var bytesTransferred = BytesTransferred;
        var error = SocketError;

        return error == SocketError.Success
            ? new ValueTask<SocketOperationResult>(new SocketOperationResult(bytesTransferred))
            : new ValueTask<SocketOperationResult>(new SocketOperationResult(CreateException(error)));
    }
}
