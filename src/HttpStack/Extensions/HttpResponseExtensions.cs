using System.Buffers;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpStack;

public static class HttpResponseExtensions
{
    public static async Task WriteAsync(this IHttpResponse response, string content, CancellationToken token = default)
    {
        if (response.Body is IStringStream stringStream)
        {
            await stringStream.WriteAsync(content);
            return;
        }

        var encoding = Encoding.UTF8;
        var length = encoding.GetMaxByteCount(content.Length);

        if (length > 4096)
        {
            length = encoding.GetByteCount(content);
        }

        var array = ArrayPool<byte>.Shared.Rent(length);

        try
        {
            var bytes = encoding.GetBytes(content, array);

            await response.Body.WriteAsync(array, 0, bytes, token);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    public static void Write(this IHttpResponse response, string content, CancellationToken token = default)
    {
        if (response.Body is IStringStream stringStream)
        {
            stringStream.Write(content);
            return;
        }

        var encoding = Encoding.UTF8;
        var length = encoding.GetMaxByteCount(content.Length);

        if (length > 4096)
        {
            length = encoding.GetByteCount(content);
        }

        var array = ArrayPool<byte>.Shared.Rent(length);

        try
        {
            var bytes = encoding.GetBytes(content, array);

            response.Body.Write(array, 0, bytes);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }
}
