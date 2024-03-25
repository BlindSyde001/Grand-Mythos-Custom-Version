using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class UIElementList<T> : IEnumerable<T> where T : MonoBehaviour
{
    [Required] public T Template;
    [SerializeField, ReadOnly] List<T> _existing = new();
    [SerializeField, ReadOnly] List<T> _pool = new();

    public void Allocate(out T element)
    {
        if (_pool.Count != 0)
        {
            element = _pool[^1];
            _pool.RemoveAt(_pool.Count - 1);
            element.gameObject.SetActive(true);
        }
        else
        {
            Template.gameObject.SetActive(false);
            element = Object.Instantiate(Template, Template.transform.parent);
            element.gameObject.SetActive(true);
        }

        _existing.Add(element);
    }

    public bool Remove(T element)
    {
        int index = _existing.IndexOf(element);
        if (index == -1)
            return false;

        element.gameObject.SetActive(false);
        _existing.RemoveAt(index);
        _pool.Add(element);
        return true;
    }

    public void Clear()
    {
        Template.gameObject.SetActive(false);
        foreach (T behaviour in _existing)
            behaviour.gameObject.SetActive(false);

        for (int i = _existing.Count - 1; i >= 0; i--) // Reverse for loop to keep the order intact since we're taking from the end when allocating
            _pool.Add(_existing[i]);
        _existing.Clear();
    }

    public List<T>.Enumerator GetEnumerator() => _existing.GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}