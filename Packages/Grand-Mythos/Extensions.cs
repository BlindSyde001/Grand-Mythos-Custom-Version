using System.Collections.Generic;

public static class Extensions
{
    public static string StringSequence(this IEnumerable<string> strings, string separator)
    {
        return string.Join(separator, strings);
    }
}
