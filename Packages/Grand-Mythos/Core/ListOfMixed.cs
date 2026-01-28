using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class ListOfMixed<T> : List<T?>, ISerializationCallbackReceiver where T : class
{
    [SerializeReference, SerializeField] List<T?> _refs = new();
    [SerializeField] List<Object?> _objects = new();

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        var cap = Math.Max(_objects.Capacity, Capacity);
        _objects.Capacity = _refs.Capacity = cap;
        //Capacity = cap;

        while (_objects.Count != Count)
            if (_objects.Count < Count)
                _objects.Add(null);
            else
                _objects.RemoveAt(_objects.Count - 1);

        while (_refs.Count != Count)
            if (_refs.Count < Count)
                _refs.Add(null);
            else
                _refs.RemoveAt(_refs.Count - 1);

        for (int i = 0; i < Count; i++)
        {
            if (this[i] is Object o)
            {
                _objects[i] = o;
                _refs[i] = null;
            }
            else
            {
                _objects[i] = null;
                _refs[i] = this[i];
            }
        }
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        Workaround();

        this.Clear();
        while (_objects.Count > this.Count)
            this.Add(null);

        for (int i = 0; i < _objects.Count; i++)
        {
            if (_refs[i] is not null)
                this[i] = _refs[i];
            else
                this[i] = _objects[i] as T;
        }
    }

    static FieldInfo? _sizeField, _itemsField;
    void Workaround()
    {
        // Fix for some weird ass stuff going on, Count == 1 when _items.Length == 0 and Capacity == 0
        _sizeField ??= GetType().BaseType!.GetField("_size", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new NullReferenceException($"Could not find '_size' field in {GetType().BaseType}");
        _itemsField ??= GetType().BaseType!.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new NullReferenceException($"Could not find '_items' field in {GetType().BaseType}");
        var array = ((Array)_itemsField.GetValue(this));
        _sizeField.SetValue(this, array.Length);
    }
}