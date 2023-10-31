using System.Text.Json.Serialization;

namespace HttpStack.Wasm.Context;

internal class ResponseContext
{
	[JsonPropertyName("status")]
	public int Status { get; set; } = 200;

	[JsonPropertyName("headers")]
	public Dictionary<string, string> Headers { get; set; } = new();
}