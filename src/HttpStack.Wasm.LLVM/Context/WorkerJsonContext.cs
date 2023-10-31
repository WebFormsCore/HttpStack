using System.Text.Json.Serialization;

namespace HttpStack.Wasm.Context;

[JsonSerializable(typeof(RequestContext))]
[JsonSerializable(typeof(ResponseContext))]
internal partial class WorkerJsonContext : JsonSerializerContext;