using System.Text;

namespace HttpStack.Collections;

public static class HeaderDictionaryExtensions
{
    public static string ToHeaderString(this IHeaderDictionary headers)
    {
        var sb = new StringBuilder();

        foreach (var header in headers)
        {
            sb.Append(header.Key);
            sb.Append(": ");

            switch (header.Value.Count)
            {
                case 0:
                    break;
                case 1:
                    sb.Append(header.Value);
                    break;
                default:
                {
                    for (var i = 0; i < header.Value.Count; i++)
                    {
                        if (i != 0)
                        {
                            sb.Append(", ");
                        }

                        sb.Append(header.Value[i]);
                    }

                    break;
                }
            }

            sb.Append("\r\n");
        }

        return sb.ToString();
    }
}
