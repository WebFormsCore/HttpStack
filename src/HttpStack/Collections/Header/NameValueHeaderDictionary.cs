namespace HttpStack.Collections;

public class NameValueHeaderDictionary : NameValueDictionary, IHeaderDictionary
{
    public string? ContentType
    {
        get => NameValueCollection["Content-Type"];
        set => NameValueCollection["Content-Type"] = value;
    }

    public long? ContentLength
    {
        get => long.TryParse(NameValueCollection["Content-Length"], out var contentLength) ? contentLength : null;
        set => NameValueCollection["Content-Length"] = value?.ToString();
    }
}
