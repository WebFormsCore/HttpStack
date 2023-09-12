using System;
using System.Threading.Tasks;

namespace HttpStack;

public interface IStringStream
{
    ValueTask WriteAsync(string content);

    ValueTask WriteAsync(ReadOnlyMemory<char> content);

    void Write(string content);

    void Write(ReadOnlySpan<char> content);
}
