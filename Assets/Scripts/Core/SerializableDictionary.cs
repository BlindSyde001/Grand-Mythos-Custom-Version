
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableDictionary<T, T2> : Dictionary<T, T2>, ISerializationCallbackReceiver
{
    [SerializeField]KeyValue[] _backingArray = Array.Empty<KeyValue>();

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        if (_backingArray.Length != Count)
            _backingArray = new KeyValue[Count];
        int i = 0;
        foreach (var keyValue in this)
            _backingArray[i++] = new() { Key = keyValue.Key, Value = keyValue.Value };
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        EnsureCapacity(4);
        Clear();
        foreach (KeyValue keyValue in _backingArray)
        {
            if (ReferenceEquals(keyValue.Key, null))
                continue;
            Add(keyValue.Key, keyValue.Value);
        }
    }

    [Serializable]
    struct KeyValue
    {
        public T Key;
        public T2 Value;
    }
}