#if NETFRAMEWORK
using System.Web;
using HttpStack.Collections;

namespace HttpStack.Owin.Collections;

public class NameValueFormCollection : NameValueDictionary, IFormCollection
{
    private readonly FormFileCollection _formFileCollection = new();

    public IFormFileCollection Files => _formFileCollection;

    public void SetHttpFileCollection(HttpFileCollectionBase collection)
    {
        _formFileCollection.SetHttpFileCollection(collection);
    }

    public override void Reset()
    {
        _formFileCollection.Reset();
        base.Reset();
    }
}
#endif