using System.Text;

namespace HttpStack;

/// <summary>
/// Extension methods for <see cref="ISession"/>.
/// </summary>
public static class SessionExtensions
{
    /// <summary>
    /// Sets an int value in the <see cref="ISession"/>.
    /// </summary>
    /// <param name="session">The <see cref="ISession"/>.</param>
    /// <param name="key">The key to assign.</param>
    /// <param name="value">The value to assign.</param>
    public static void SetInt32(this ISession session, string key, int value)
    {
        if (session is IObjectSession objectSession)
        {
            objectSession.Set(key, value);
            return;
        }

        var bytes = new[]
        {
            (byte)(value >> 24),
            (byte)(0xFF & (value >> 16)),
            (byte)(0xFF & (value >> 8)),
            (byte)(0xFF & value)
        };
        session.Set(key, bytes);
    }

    /// <summary>
    /// Gets an int value from <see cref="ISession"/>.
    /// </summary>
    /// <param name="session">The <see cref="ISession"/>.</param>
    /// <param name="key">The key to read.</param>
    public static int? GetInt32(this ISession session, string key)
    {
        if (session is IObjectSession objectSession)
        {
            if (objectSession.TryGetValue(key, out var value))
            {
                return value switch
                {
                    int intValue => intValue,
                    string when int.TryParse(value.ToString(), out var intValue) => intValue,
                    byte byteValue => byteValue,
                    long longValue => (int)longValue,
                    byte[] bytes => BytesToInt(bytes),
                    _ => null
                };
            }

            return null;
        }

        return BytesToInt(session.Get(key));

        static int? BytesToInt(byte[]? data)
        {
            if (data == null || data.Length < 4)
            {
                return null;
            }

            return data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3];
        }
    }

    /// <summary>
    /// Sets a <see cref="string"/> value in the <see cref="ISession"/>.
    /// </summary>
    /// <param name="session">The <see cref="ISession"/>.</param>
    /// <param name="key">The key to assign.</param>
    /// <param name="value">The value to assign.</param>
    public static void SetString(this ISession session, string key, string value)
    {
        if (session is IObjectSession objectSession)
        {
            objectSession.Set(key, value);
            return;
        }

        session.Set(key, Encoding.UTF8.GetBytes(value));
    }

    /// <summary>
    /// Gets a string value from <see cref="ISession"/>.
    /// </summary>
    /// <param name="session">The <see cref="ISession"/>.</param>
    /// <param name="key">The key to read.</param>
    public static string? GetString(this ISession session, string key)
    {
        if (session is IObjectSession objectSession)
        {
            if (objectSession.TryGetValue(key, out var value))
            {
                return value switch
                {
                    null => null,
                    string stringValue => stringValue,
                    byte[] bytes => Encoding.UTF8.GetString(bytes),
                    _ => value.ToString()
                };
            }

            return null;
        }

        var data = session.Get(key);
        if (data == null)
        {
            return null;
        }

        return Encoding.UTF8.GetString(data);
    }

    /// <summary>
    /// Gets a byte-array value from <see cref="ISession"/>.
    /// </summary>
    /// <param name="session">The <see cref="ISession"/>.</param>
    /// <param name="key">The key to read.</param>
    public static byte[]? Get(this ISession session, string key)
    {
        if (session is IObjectSession objectSession)
        {
            if (objectSession.TryGetValue(key, out var value))
            {
                return value switch
                {
                    null => null,
                    byte[] bytes => bytes,
                    string stringValue => Encoding.UTF8.GetBytes(stringValue),
                    _ => Encoding.UTF8.GetBytes(value.ToString() ?? "")
                };
            }

            return null;
        }
        else
        {
            session.TryGetValue(key, out var value);
            return value;
        }
    }
}