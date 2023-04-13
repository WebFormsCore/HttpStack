using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace HttpStack.Collections;

public interface IHeaderDictionary : IDictionary<string, StringValues>
{
    string? ContentType { get; set; }

    long? ContentLength { get; set; }
}

public class HeaderDictionary : Dictionary<string, StringValues>, IHeaderDictionary
{
    public HeaderDictionary()
        : base(StringComparer.OrdinalIgnoreCase)
    {
    }

    public HeaderDictionary(IDictionary<string, StringValues> dictionary)
        : base(dictionary, StringComparer.OrdinalIgnoreCase)
    {
    }

#if NETSTANDARD2_0
    public HeaderDictionary(IEnumerable<KeyValuePair<string, StringValues>> collection)
        : base(StringComparer.OrdinalIgnoreCase)
    {
        foreach (var item in collection)
        {
            Add(item.Key, item.Value);
        }
    }
#else
    public HeaderDictionary(IEnumerable<KeyValuePair<string, StringValues>> collection)
        : base(collection, StringComparer.OrdinalIgnoreCase)
    {
    }
#endif

    public HeaderDictionary(IEnumerable<KeyValuePair<string, string>> collection)
        : this(collection.Select(x => new KeyValuePair<string, StringValues>(x.Key, x.Value)))
    {
    }

    public string? ContentType
    {
        get => TryGetValue("Content-Type", out var value) ? value.ToString() : null;
        set => this["Content-Type"] = value;
    }

    public long? ContentLength
    {
        get => TryGetValue("Content-Length", out var value) && long.TryParse(value.ToString(), out var contentLength) ? contentLength : null;
        set => this["Content-Length"] = value.ToString();
    }
}
