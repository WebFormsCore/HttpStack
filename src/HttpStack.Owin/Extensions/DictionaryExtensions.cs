using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HttpStack.Owin;

internal static class DictionaryExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetRequired<T>(this IDictionary<string, object> env, string key)
    {
        if (!env.TryGetValue(key, out var valueObj) || valueObj is not T value)
        {
            throw new InvalidOperationException($"The environment does not contain a value for the key '{key}'.");
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull("defaultValue")]
    public static T? GetOptional<T>(this IDictionary<string, object> env, string key, T? defaultValue = default)
        where T : notnull
    {
        if (!env.TryGetValue(key, out var valueObj) || valueObj is not T value)
        {
            return defaultValue;
        }

        return value;
    }
}
