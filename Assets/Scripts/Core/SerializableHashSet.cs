using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableHashSet<T> : HashSet<T>, ISerializationCallbackReceiver
{
    [SerializeField]T[] _backingArray = Array.Empty<T>();

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        if (_backingArray == null || _backingArray.Length != Count)
            _backingArray = new T[Count];
        CopyTo(_backingArray);
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        EnsureCapacity(4);
        Clear();
        foreach (var val in _backingArray)
            Add(val);
    }
}