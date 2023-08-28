namespace HttpStack.Collections;

public sealed class RequestHeaderDictionary : TypedHeaderDictionary, IRequestHeaderDictionary
{
    public RequestHeaderDictionary()
        : base(new HeaderDictionary())
    {
    }

    public RequestHeaderDictionary(IHeaderDictionary dictionary)
        : base(dictionary)
    {
    }
}
