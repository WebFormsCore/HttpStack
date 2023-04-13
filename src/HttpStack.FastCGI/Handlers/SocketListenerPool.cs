using System.Diagnostics;

namespace HttpStack.FastCGI.Handlers;

internal sealed class SocketListenerPool : IDisposable
{
    private readonly ObjectWrapper[] _items;
    private SocketListener? _firstItem;
    private bool _disposed;

    public SocketListenerPool(int maximumRetained)
    {
        _items = new ObjectWrapper[maximumRetained - 1];
    }

    public SocketListener Get()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(SocketListenerPool));
        }

        var item = _firstItem;

        if (item is not null && Interlocked.CompareExchange(ref _firstItem, null, item) == item)
        {
            return item;
        }

        var items = _items;
        for (var i = 0; i < items.Length; i++)
        {
            item = items[i].Element;
            if (item is not null && Interlocked.CompareExchange(ref items[i].Element, null, item) == item)
            {
                return item;
            }
        }

        item = new SocketListener();
        return item;
    }

    public void Return(SocketListener obj)
    {
        if (_disposed)
        {
            obj.Dispose();
            return;
        }

        obj.Reset();

        if (_firstItem is null && Interlocked.CompareExchange(ref _firstItem, obj, null) is null)
        {
            if (_disposed)
            {
                obj.Dispose();
            }

            return;
        }

        var items = _items;
        for (var i = 0; i < items.Length; i++)
        {
            if (Interlocked.CompareExchange(ref items[i].Element, obj, null) is not null)
            {
                continue;
            }

            if (_disposed)
            {
                obj.Dispose();
            }

            return;
        }

        obj.Dispose();
    }

    // PERF: the struct wrapper avoids array-covariance-checks from the runtime when assigning to elements of the array.
    [DebuggerDisplay("{Element}")]
    private struct ObjectWrapper
    {
        public SocketListener? Element;
    }

    public void Dispose()
    {
        _disposed = true;
        _firstItem?.Socket?.Dispose();

        foreach (var item in _items)
        {
            item.Element?.Socket?.Dispose();
        }
    }
}
