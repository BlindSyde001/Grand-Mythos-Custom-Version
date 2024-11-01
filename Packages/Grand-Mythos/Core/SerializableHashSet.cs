using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableHashSet<T> : ISet<T>, ISerializationCallbackReceiver
{
    [SerializeField]T[] _backingArray = Array.Empty<T>();
    HashSet<T> _proxy;

    public SerializableHashSet()
    {
        _proxy = new();
    }

    public SerializableHashSet(IEqualityComparer<T> comparer)
    {
        _proxy = new(comparer);
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        if (_backingArray == null || _backingArray.Length != Count)
            _backingArray = new T[Count];
        _proxy.CopyTo(_backingArray);
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        Clear();
        foreach (var val in _backingArray)
            _proxy.Add(val);
    }

    public HashSet<T>.Enumerator GetEnumerator() => _proxy.GetEnumerator();
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => _proxy.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    public bool TryGetValue(T item, out T matchingItem) => _proxy.TryGetValue(item, out matchingItem);
    public bool Contains(T item) => _proxy.Contains(item);
    public bool Add(T item) => _proxy.Add(item);
    public bool Remove(T item) => _proxy.Remove(item);
    void ICollection<T>.Add(T item) => Add(item);

    public void Clear() => _proxy.Clear();

    public void CopyTo(T[] array, int arrayIndex) => _proxy.CopyTo(array, arrayIndex);
    public int Count => _proxy.Count;
    bool ICollection<T>.IsReadOnly => ((ISet<T>)_proxy).IsReadOnly;

    public void ExceptWith(IEnumerable<T> other) => _proxy.ExceptWith(other);
    public void IntersectWith(IEnumerable<T> other) => _proxy.IntersectWith(other);
    public bool IsProperSubsetOf(IEnumerable<T> other) => _proxy.IsProperSubsetOf(other);
    public bool IsProperSupersetOf(IEnumerable<T> other) => _proxy.IsProperSupersetOf(other);
    public bool IsSubsetOf(IEnumerable<T> other) => _proxy.IsSubsetOf(other);
    public bool IsSupersetOf(IEnumerable<T> other) => _proxy.IsSupersetOf(other);
    public bool Overlaps(IEnumerable<T> other) => _proxy.Overlaps(other);
    public bool SetEquals(IEnumerable<T> other) => _proxy.SetEquals(other);
    public void SymmetricExceptWith(IEnumerable<T> other) => _proxy.SymmetricExceptWith(other);
    public void UnionWith(IEnumerable<T> other) => _proxy.UnionWith(other);
}