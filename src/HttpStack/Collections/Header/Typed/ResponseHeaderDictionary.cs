using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace HttpStack.Collections;

public sealed class ResponseHeaderDictionary : TypedHeaderDictionary, IResponseHeaderDictionary
{
    public ResponseHeaderDictionary()
        : base(new HeaderDictionary())
    {
    }

    public ResponseHeaderDictionary(IHeaderDictionary dictionary)
        : base(dictionary)
    {
    }
}