#if NET8_0_OR_GREATER
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;

namespace HttpStack;

internal sealed class MetricsBuilder : IMetricsBuilder
{
	public MetricsBuilder(IServiceCollection services)
	{
		Services = services;
	}

	public IServiceCollection Services { get; }
}
#endif