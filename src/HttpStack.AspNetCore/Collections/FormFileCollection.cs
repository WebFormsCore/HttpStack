using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using HttpStack.Collections;
using HttpStack.Forms;
using Microsoft.Extensions.ObjectPool;

namespace HttpStack.AspNetCore.Collections;

public class FormFileCollection : IFormFileCollection
{
    private static readonly ObjectPool<FormFile> FormFilePool = new DefaultObjectPool<FormFile>(new DefaultPooledObjectPolicy<FormFile>());
    private Microsoft.AspNetCore.Http.IFormFileCollection? _formCollection;
    private readonly List<FormFile> _formFiles = new();

    public void Reset()
    {
        foreach (var file in _formFiles)
        {
            file.Reset();
            FormFilePool.Return(file);
        }

        _formFiles.Clear();
        _formCollection = null!;
    }

    public void SetFormFileCollection(Microsoft.AspNetCore.Http.IFormFileCollection collection)
    {
        _formCollection = collection;
    }

    private void Load()
    {
        Debug.Assert(_formCollection != null);

        var count = _formCollection.Count;

        for (var i = 0; i < count; i++)
        {
            var formFile = _formCollection[i];
            var file = FormFilePool.Get();
            file.SetFormFile(formFile);
            _formFiles.Add(file);
        }
    }

    public IEnumerator<IFormFile> GetEnumerator()
    {
        Load();
        return _formFiles.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _formCollection?.Count ?? 0;

    public IFormFile this[int index]
    {
        get
        {
            Load();
            return _formFiles[index];
        }
    }

    public IFormFile? this[string name] => GetFile(name);

    public IFormFile? GetFile(string name)
    {
        Load();

        foreach (var file in _formFiles)
        {
            if (string.Equals(name, file.Name, StringComparison.OrdinalIgnoreCase))
            {
                return file;
            }
        }

        return null;
    }

    public IReadOnlyList<IFormFile> GetFiles(string name)
    {
        Load();

        var files = new List<IFormFile>();

        foreach (var file in _formFiles)
        {
            if (string.Equals(name, file.Name, StringComparison.OrdinalIgnoreCase))
            {
                files.Add(file);
            }
        }

        return files;
    }

    private class FormFile : IFormFile
    {
        private readonly HeaderCollectionImpl _headers = new();
        private Microsoft.AspNetCore.Http.IFormFile? _formFile;

        public void SetFormFile(Microsoft.AspNetCore.Http.IFormFile formFile)
        {
            _formFile = formFile;
            _headers.SetHeaderDictionary(formFile.Headers);
        }

        public string? ContentType => _formFile?.ContentType;

        public string? FileName => _formFile?.FileName;

        public IHeaderDictionary Headers => _headers;

        public long Length => _formFile?.Length ?? 0;

        public string? Name => _formFile?.Name;

        public Stream OpenReadStream()
        {
            return _formFile!.OpenReadStream();
        }

        public void Reset()
        {
            _formFile = null!;
            _headers.Reset();
        }
    }
}
