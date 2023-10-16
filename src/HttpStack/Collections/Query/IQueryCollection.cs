using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace HttpStack.Collections;

public interface IQueryCollection : IReadOnlyDictionary<string, StringValues>
{
	
}
