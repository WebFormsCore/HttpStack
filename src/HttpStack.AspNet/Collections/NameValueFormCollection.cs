using System;
using System.Web;
using HttpStack.Collections;

namespace HttpStack.AspNet.Collections;

public class NameValueFormCollection : NameValueDictionary, IFormCollection
{
    private readonly FormFileCollection _formFileCollection = new();

    public IFormFileCollection Files => _formFileCollection;

    public void SetHttpFileCollection(HttpFileCollection collection)
    {
        _formFileCollection.SetHttpFileCollection(collection);
    }

    public override void Reset()
    {
        _formFileCollection.Reset();
        base.Reset();
    }
}
