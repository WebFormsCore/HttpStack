using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace HttpStack.Collections;

public interface IHeaderDictionary : IDictionary<string, StringValues>
{
    string? ContentType { get; set; }

    long? ContentLength { get; set; }
}
