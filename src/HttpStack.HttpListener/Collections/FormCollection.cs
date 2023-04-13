using System;
using HttpStack.Collections;

namespace HttpStack.NetHttpListener.Collections;

public class FileCollection : NameValueDictionary, IFormCollection
{
    public IFormFileCollection Files => throw new NotImplementedException();
}
