using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Web;
using HttpStack.Collections;
using HttpStack.Host;
using HttpStack.Wasm.Context;

namespace HttpStack.Wasm;

public static class WasmInterop
{
	internal static IHttpStack<WasmContext> Stack { get; set; } = null!;

	public static unsafe IntPtr BeginRequest(int contextLength, byte* contextPtr, int bodyLength, byte* bodyPtr)
	{
		var span = new Span<byte>(contextPtr, contextLength);
		var context = JsonSerializer.Deserialize(span, WorkerJsonContext.Default.RequestContext)!;
		using var body = new UnmanagedMemoryStream(bodyPtr, bodyLength);

		IFormCollection form = new FormCollection();

		if (context.Method == "POST" &&
		    context.Headers.TryGetValue("content-type", out var contentType) &&
		    contentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
		{
			var bodyAsString = Encoding.UTF8.GetString(bodyPtr, bodyLength);

			form = new NameValueFormCollection(HttpUtility.ParseQueryString(bodyAsString));
		}

		return CreateResponse(context, body, form).GetAwaiter().GetResult();
	}

	public static void EndRequest(IntPtr ptr)
	{
		Marshal.FreeHGlobal(ptr);
	}

	private static async ValueTask<IntPtr> CreateResponse(
		RequestContext requestContext,
		Stream requestBody,
		IFormCollection form)
	{
		var context = new WasmContext(requestContext, requestBody, form);
		var httpContext = Stack.CreateContext(context);

		await Stack.ExecuteAsync(httpContext);

		var pointer = Alloc(context.Stream, context.Response);

		await Stack.DisposeAsync(httpContext);

		return pointer;
	}

	private static unsafe IntPtr Alloc(MemoryStream stream, ResponseContext response)
	{
		stream.Flush();

		var bodyLength = (uint) stream.Length;
		JsonSerializer.Serialize(stream, response, WorkerJsonContext.Default.ResponseContext);
		stream.Flush();

		var contextLength = (uint) stream.Length - bodyLength;

		stream.Position = 0;

		Span<byte> source = stream.TryGetBuffer(out var segment) ? segment : stream.ToArray();

		var length = source.Length + 8;
		var ptr = Marshal.AllocHGlobal(length);
		var p = (byte*)ptr.ToPointer();
		var target = new Span<byte>(p, length);

		BinaryPrimitives.WriteUInt32LittleEndian(target, bodyLength);
		BinaryPrimitives.WriteUInt32LittleEndian(target.Slice(4), contextLength);
		source.CopyTo(target.Slice(8));

		return ptr;
	}
}
