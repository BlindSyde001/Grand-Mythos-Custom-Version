using UnityEngine;

public static class InspectorHelper
{
    public static void AutoAssign<T>(this MonoBehaviour b, ref T field)
    {
        if (field == null && b.GetComponent<T>() is { } v)
        {
            field = v;
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(b);
            #endif
            Debug.LogWarning($"Automatically assigned field {typeof(T)} for {b} to {v}", b);
        }
    }
}