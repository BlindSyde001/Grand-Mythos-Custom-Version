using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QueryPool
{
    public static RecycledEnum<T> PooledGetInChildren<T>(this GameObject go, bool includeInactive = false) where T : Component
    {
        RecycledEnum<T>.Borrow(out var enu, out var buffer);
        go.GetComponentsInChildren(includeInactive, buffer);
        enu.Reset();
        return enu;
    }

    public static RecycledEnum<T> PooledGetInParent<T>(this GameObject go, bool includeInactive = false) where T : Component
    {
        RecycledEnum<T>.Borrow(out var enu, out var buffer);
        go.GetComponentsInParent(includeInactive, buffer);
        enu.Reset();
        return enu;
    }

    public static RecycledEnum<T> PooledGet<T>(this GameObject go) where T : Component
    {
        RecycledEnum<T>.Borrow(out var enu, out var buffer);
        go.GetComponents(buffer);
        enu.Reset();
        return enu;
    }

    public static RecycledEnum<T> PooledGetInChildren<T>(this Component go, bool includeInactive = false) where T : Component
    {
        RecycledEnum<T>.Borrow(out var enu, out var buffer);
        go.GetComponentsInChildren(includeInactive, buffer);
        enu.Reset();
        return enu;
    }

    public static RecycledEnum<T> PooledGetInParent<T>(this Component go, bool includeInactive = false) where T : Component
    {
        RecycledEnum<T>.Borrow(out var enu, out var buffer);
        go.GetComponentsInParent(includeInactive, buffer);
        enu.Reset();
        return enu;
    }

    public static RecycledEnum<T> PooledGet<T>(this Component go) where T : Component
    {
        RecycledEnum<T>.Borrow(out var enu, out var buffer);
        go.GetComponents(buffer);
        enu.Reset();
        return enu;
    }

    public static IDisposable BorrowList<T>(out List<T> list)
    {
        RecycledEnum<T>.Borrow(out var enu, out list);
        return enu;
    }

    public static IDisposable TemporaryCopy<T>(this ICollection<T> sourceToCopy, out List<T> list)
    {
        RecycledEnum<T>.Borrow(out var enu, out list);
        list.AddRange(sourceToCopy);
        return enu;
    }

    public static IDisposable TemporaryCopy<T, T2>(this IDictionary<T, T2> sourceToCopy, out List<T> list)
    {
        RecycledEnum<T>.Borrow(out var enu, out list);
        foreach (var kvp in sourceToCopy)
            list.Add(kvp.Key);
        return enu;
    }

    public class RecycledEnum<T> : IEnumerable<T>, IEnumerator<T>
    {
        readonly List<T> _buffer = new();
        List<T>.Enumerator _inner;
        bool _disposed = false;

        public T Current => _inner.Current!;

        private RecycledEnum() { }

        public void Dispose()
        {
            _disposed = true;
            _pool.Push(this);
            _buffer.Clear();
        }

        public bool MoveNext()
        {
            if (_disposed)
                throw new ObjectDisposedException("This object was sent for reuse, do not query it after disposing");
            return _inner.MoveNext();
        }

        public void Reset()
        {
            _inner = _buffer.GetEnumerator();
        }

        object IEnumerator.Current => Current!;
        public RecycledEnum<T> GetEnumerator() => this;
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        static readonly Stack<RecycledEnum<T>> _pool = new();
        public static void Borrow(out RecycledEnum<T> enu, out List<T> buffer)
        {
            enu = _pool.TryPop(out var v) ? v : new RecycledEnum<T>();
            enu._disposed = false;
            buffer = enu._buffer;
        }
    }
}