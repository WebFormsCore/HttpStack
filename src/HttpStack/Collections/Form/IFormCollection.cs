using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using HttpStack.Forms;
using Microsoft.Extensions.Primitives;

namespace HttpStack.Collections;

public interface IFormCollection : IReadOnlyDictionary<string, StringValues>
{
    IFormFileCollection Files { get; }
}

public interface IFormFileCollection : IReadOnlyList<IFormFile>
{
    IFormFile? this[string name] { get; }

    IFormFile? GetFile(string name);

    IReadOnlyList<IFormFile> GetFiles(string name);
}
