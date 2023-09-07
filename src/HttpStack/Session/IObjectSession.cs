using System.Diagnostics.CodeAnalysis;

namespace HttpStack;

/// <summary>
/// Stores user data while the user browses a web application. Session state uses a store maintained by the application
/// to persist data across requests from a client. The session data is backed by a cache and considered ephemeral data.
/// </summary>
public interface IObjectSession
{
    /// <summary>
    /// Set the given key and value in the current session. This will throw if the session
    /// was not established prior to sending the response.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    void Set(string key, object? value);

    /// <summary>
    /// Retrieve the value of the given key, if present.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns>The retrieved value.</returns>
    bool TryGetValue(string key, [NotNullWhen(true)] out object? value);
}
