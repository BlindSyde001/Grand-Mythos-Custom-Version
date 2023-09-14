using System;
using UnityEngine;

/// <summary> Globally unique identifier </summary>
[Serializable]
public struct guid : IComparable<guid>, IEquatable<guid>
{
    [SerializeField, HideInInspector]
    ulong a, b;

    public int CompareTo(guid other)
    {
        return (a, b).CompareTo((other.a, other.b));
    }

    public static bool operator ==(guid a, guid b) => a.Equals(b);
    public static bool operator !=(guid a, guid b) => a.Equals(b) == false;
    public bool Equals(guid other) => this.a == other.a && this.b == other.b;
    public override bool Equals(object obj) => obj is guid g && Equals(g);
    public override int GetHashCode() => (a, b).GetHashCode();
}