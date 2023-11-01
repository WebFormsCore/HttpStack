using HttpStack.Collections;
using HttpStack.Wasm.Context;

namespace HttpStack.Wasm;

internal struct WasmContext
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
}
