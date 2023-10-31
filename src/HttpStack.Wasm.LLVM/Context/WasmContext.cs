using HttpStack.Collections;
using HttpStack.Wasm.Context;

namespace HttpStack.Wasm;

internal class WasmContext
{
	public WasmContext(RequestContext request, Stream requestBody, IFormCollection form)
	{
		Request = request;
		RequestBody = requestBody;
		Form = form;
	}

	public RequestContext Request { get; }

	public Stream RequestBody { get; }

	public IFormCollection Form { get; }

	public ResponseContext Response { get; set; } = null!;

	public MemoryStream Stream { get; set; } = null!;
}
