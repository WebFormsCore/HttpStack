using System.Threading.Tasks;

namespace HttpStack.Host;

public interface IHttpStack<in TInnerContext>
{
    ValueTask ProcessRequestAsync(TInnerContext context);
}
