using System;
using UnityEngine;

/// <summary> Globally unique identifier </summary>
[Serializable]
public struct guid : IComparable<guid>, IEquatable<guid>
{
    [SerializeField, HideInInspector]
    ulong a, b;

    public guid(string guidString)
    {
        this = new Guid(guidString);
    }

    public guid(Guid systemGuid)
    {
        this = systemGuid;
    }

    public int CompareTo(guid other)
    {
        return (a, b).CompareTo((other.a, other.b));
    }

    public static implicit operator guid(Guid systemGuid)
    {
        unsafe
        {
            if (sizeof(Guid) != sizeof(guid))
                throw new InvalidOperationException($"Size mismatch between {typeof(Guid)} and {nameof(guid)}, {sizeof(Guid)} != {sizeof(guid)}");
            return *(guid*)&systemGuid;
        }
    }

    public static implicit operator Guid(guid systemGuid)
    {
        unsafe
        {
            if (sizeof(Guid) != sizeof(guid))
                throw new InvalidOperationException($"Size mismatch between {typeof(Guid)} and {nameof(guid)}, {sizeof(Guid)} != {sizeof(guid)}");
            return *(Guid*)&systemGuid;
        }
    }

    #if UNITY_EDITOR
    public static implicit operator guid(UnityEditor.GUID systemGuid)
    {
        unsafe
        {
            if (sizeof(UnityEditor.GUID) != sizeof(guid))
                throw new InvalidOperationException($"Size mismatch between {typeof(UnityEditor.GUID)} and {nameof(guid)}, {sizeof(UnityEditor.GUID)} != {sizeof(guid)}");
            return *(guid*)&systemGuid;
        }
    }
    #endif

    public static bool operator ==(guid a, guid b) => a.Equals(b);
    public static bool operator !=(guid a, guid b) => a.Equals(b) == false;
    public bool Equals(guid other) => this.a == other.a && this.b == other.b;
    public override bool Equals(object obj) => obj is guid g && Equals(g);
    public override int GetHashCode() => (a, b).GetHashCode();

    public override string ToString() => ((Guid)this).ToString();
}