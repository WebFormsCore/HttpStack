using System;
using System.Collections;
using System.Collections.Generic;
using HttpStack.Forms;

namespace HttpStack.Collections;

public class FormFileCollection : IFormFileCollection
{
    private readonly List<IFormFile> _files = new();
    private readonly Dictionary<string, List<IFormFile>> _filesByName = new();

    public void Reset()
    {
        foreach (var file in _files)
        {
            if (file is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _files.Clear();
        _filesByName.Clear();
    }

    IEnumerator<IFormFile> IEnumerable<IFormFile>.GetEnumerator()
    {
        return _files.GetEnumerator();
    }

    public IEnumerator GetEnumerator()
    {
        return ((IEnumerable)_files).GetEnumerator();
    }

    public int Count => _files.Count;

    public IFormFile this[int index] => _files[index];

    public IFormFile? this[string name] => GetFile(name);

    public IFormFile? GetFile(string name)
    {
        if (_filesByName.TryGetValue(name, out var files))
        {
            return files[0];
        }

        return null;
    }

    public IReadOnlyList<IFormFile> GetFiles(string name)
    {
        if (_filesByName.TryGetValue(name, out var files))
        {
            return files;
        }

        return Array.Empty<IFormFile>();
    }

    public void Add(IFormFile file)
    {
        _files.Add(file);

        if (file.Name is not {} name)
        {
            return;
        }

        if (!_filesByName.TryGetValue(name, out var filesByName))
        {
            filesByName = new List<IFormFile>();
            _filesByName.Add(name, filesByName);
        }

        filesByName.Add(file);
    }
}
