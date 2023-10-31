using System.Runtime.InteropServices;
using HttpStack.Wasm;

namespace HttpStack.Examples.Wasm;

public static class Interop
{
	[UnmanagedCallersOnly(EntryPoint = "Initialize")]
	public static void Initialize()
	{
		var builder = new HttpApplicationBuilder();
		var app = builder.Build();

		app.Run(async context =>
		{
			if (context.Request.Path != "/")
			{
				context.Response.StatusCode = 404;
				return;
			}

			context.Response.ContentType = "text/plain";
			await context.Response.WriteAsync("Hello World!");
			await context.Response.Body.FlushAsync();
		});

		app.RegisterWasmStack();
	}

	[UnmanagedCallersOnly(EntryPoint = "BeginRequest")]
	public static unsafe IntPtr BeginRequest(int contextLength, byte* contextPtr, int bodyLength, byte* bodyPtr)
	{
		return WasmInterop.BeginRequest(contextLength, contextPtr, bodyLength, bodyPtr);
	}

	[UnmanagedCallersOnly(EntryPoint = "EndRequest")]
	public static void EndRequest(IntPtr ptr)
	{
		WasmInterop.EndRequest(ptr);
	}
}
