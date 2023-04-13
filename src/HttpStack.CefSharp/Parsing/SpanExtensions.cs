using System;

namespace HttpStack.Extensions;

internal static class SpanExtensions
{
    public static bool EqualsLowerCased(this ReadOnlySpan<byte> span, ReadOnlySpan<byte> value)
    {
        if (span.Length != value.Length)
        {
            return false;
        }

        for (var i = 0; i < span.Length; i++)
        {
            if (span[i] != value[i] && span[i] != value[i] - 32)
            {
                return false;
            }
        }

        return true;
    }

    public static ReadOnlySpan<byte> Trim(this ReadOnlySpan<byte> span)
    {
        var start = 0;
        var end = span.Length - 1;

        while (start < span.Length && span[start] == ' ')
        {
            start++;
        }

        while (end >= 0 && span[end] == ' ')
        {
            end--;
        }

        return start > 0 || end < span.Length - 1 ? span.Slice(start, end - start + 1) : default;
    }

    public static ReadOnlySpan<byte> TrimStart(this ReadOnlySpan<byte> span)
    {
        var start = 0;

        while (start < span.Length && span[start] == ' ')
        {
            start++;
        }

        return start > 0 ? span.Slice(start) : span;
    }

    public static ReadOnlySpan<byte> TrimEnd(this ReadOnlySpan<byte> span)
    {
        var initial = span.Length - 1;
        var end = initial;

        while (end >= 0 && span[end] == ' ')
        {
            end--;
        }

        return end < initial ? span.Slice(0, end + 1) : span;
    }
}
