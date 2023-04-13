using System.Buffers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpStack;

public static class HttpResponseExtensions
{
    public static async Task WriteAsync(this IHttpResponse response, string content, CancellationToken token = default)
    {
        var encoding = Encoding.UTF8;
        var length = encoding.GetMaxByteCount(content.Length);
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
}
