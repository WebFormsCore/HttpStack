using Microsoft.Extensions.DependencyInjection;

namespace HttpStack.Host;

public interface IContextScopeProvider<in TInnerContext>
{
    IServiceScope CreateScope(TInnerContext context);
}
