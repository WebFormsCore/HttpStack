using System;
using HttpStack.Collections;

namespace HttpStack.AspNet.Collections;

public class FileCollection : NameValueDictionary, IFormCollection
{
    public IFormFileCollection Files => throw new NotImplementedException();
}
