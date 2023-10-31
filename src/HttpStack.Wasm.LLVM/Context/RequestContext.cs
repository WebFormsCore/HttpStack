using System.Text.Json.Serialization;

namespace HttpStack.Wasm.Context;

internal class RequestContext
{
	[JsonPropertyName("url")]
	public required string Url { get; set; }

	[JsonPropertyName("method")]
	public required string Method { get; set; }

	[JsonPropertyName("headers")]
	public Dictionary<string, string> Headers { get; set; } = new();
}