using HttpStack.Host;

namespace HttpStack.Wasm;

public static class StackExtensions
{
    public static void RegisterWasmStack(
        this IHttpStackBuilder stackBuilder,
        MiddlewareDelegate? defaultHandler = null)
    {
        WasmInterop.Stack = stackBuilder.CreateStack<HttpContextImpl, WasmContext>(scopeProvider: null, defaultHandler);
    }
}
