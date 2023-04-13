using System.IO;
using HttpStack.Collections;

namespace HttpStack.Forms;

public interface IFormFile
{
    string? ContentType { get; }
    string? FileName { get; }
    IHeaderDictionary Headers { get; }
    long Length { get; }
    string? Name { get; }
    Stream OpenReadStream();
}
