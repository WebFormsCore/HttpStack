using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HttpStack.Collections;
using Microsoft.Extensions.Primitives;

namespace HttpStack.AspNetCore.Collections;

internal class FormCollectionDictionary : IFormCollection
{
    private Microsoft.AspNetCore.Http.IFormCollection? _formCollection;
    private readonly FormFileCollection _formFiles = new();

    public IFormFileCollection Files => _formFiles;

    public void Reset()
    {
        _formCollection = null!;
        _formFiles.Reset();
    }

    public void SetFormCollection(Microsoft.AspNetCore.Http.IFormCollection nameValueCollection)
    {
        _formCollection = nameValueCollection;
        _formFiles.SetFormFileCollection(nameValueCollection.Files);
    }

    public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
    {
        return _formCollection?.GetEnumerator() ?? Enumerable.Empty<KeyValuePair<string, StringValues>>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _formCollection?.Count ?? 0;
    public bool ContainsKey(string key)
    {
        return _formCollection?.ContainsKey(key) ?? false;
    }

    public bool TryGetValue(string key, out StringValues value)
    {
        if (_formCollection is not null)
        {
            return _formCollection.TryGetValue(key, out value);
        }

        value = default!;
        return false;

    }

    public StringValues this[string key] => _formCollection?[key] ?? default;

    public IEnumerable<string> Keys => _formCollection?.Keys ?? Enumerable.Empty<string>();
    public IEnumerable<StringValues> Values => _formCollection?.Keys.Select(key => _formCollection[key]) ?? Enumerable.Empty<StringValues>();
}
