using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static string StringSequence(this IEnumerable<string> strings, string separator)
    {
        return string.Join(separator, strings);
    }

    public static void ForceDestroy(this Object obj)
    {
        if (Application.isPlaying)
            Object.Destroy(obj);
        else
            Object.DestroyImmediate(obj);
    }
}
