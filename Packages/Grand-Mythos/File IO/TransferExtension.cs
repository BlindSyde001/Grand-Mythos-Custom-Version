using System;
using System.Collections.Generic;
using UnityEngine;

public static class TransferExtension
{
    public static void Value<T>(this SavingSystem.Transfer type, ref T handler, ref T source) where T : unmanaged
    {
        if (type == SavingSystem.Transfer.PullFromSource)
            handler = source;
        else
            source = handler;
    }

    public static void Identifiable<T>(this SavingSystem.Transfer type, ref guid handler, ref T? source) where T : IdentifiableScriptableObject
    {
        if (type == SavingSystem.Transfer.PullFromSource)
        {
            handler = source != null ? source.Guid : new guid();
        }
        else
        {
            if (IdentifiableDatabase.TryGet(handler, out T? item))
                source = item;
            else
                Debug.LogError($"Could not find '{typeof(T)}' with id '{handler}' in the database");
        }
    }

    public static void Collection<T, T2>(this SavingSystem.Transfer type, ref T handler, ref T source) where T : ICollection<T2>, new() where T2 : unmanaged
    {
        if (type.TryTransferAsNull(ref handler, ref source))
            return;

        type.EnsureNotNull(ref handler, ref source);
        if (type == SavingSystem.Transfer.PullFromSource)
        {
            handler.Clear();
            foreach (T2 item in source)
                handler.Add(item);
        }
        else
        {
            source.Clear();
            foreach (T2 item in handler)
                source.Add(item);
        }
    }

    public static void Collection<T>(this SavingSystem.Transfer type, ref T handler, ref T source) where T : ICollection<string>, new()
    {
        if (type.TryTransferAsNull(ref handler, ref source))
            return;

        type.EnsureNotNull(ref handler, ref source);
        if (type == SavingSystem.Transfer.PullFromSource)
        {
            handler ??= new();
            handler.Clear();
            foreach (var item in source)
                handler.Add(item);
        }
        else
        {
            source ??= new();
            source.Clear();
            foreach (var item in handler)
                source.Add(item);
        }
    }

    public static void Identifiables<T, T2, T3>(this SavingSystem.Transfer type, ref T handler, ref T2 source) where T : ICollection<guid>, new()  where T2 : ICollection<T3>, new() where T3 : IdentifiableScriptableObject
    {
        if (type.TryTransferAsNull(ref handler, ref source))
            return;

        type.EnsureNotNull(ref handler, ref source);
        if (type == SavingSystem.Transfer.PullFromSource)
        {
            handler.Clear();
            foreach (var item in source)
                handler.Add(item.Guid);
        }
        else
        {
            source.Clear();
            foreach (var item in handler)
            {
                if (IdentifiableDatabase.TryGet(item, out T3? identifiable) && identifiable is not null)
                    source.Add(identifiable);
                else
                    Debug.LogError($"Could not find '{typeof(T3)}' with id '{handler}' in the database");
            }
        }
    }

    public static void Identifiables<T>(this SavingSystem.Transfer type, ref guid[] handler, ref T[] source) where T : IdentifiableScriptableObject
    {
        if (type.TryTransferAsNull(ref handler, ref source))
            return;

        if (type == SavingSystem.Transfer.PullFromSource)
        {
            handler = new guid[source.Length];
            for (int i = 0; i < source.Length; i++)
                handler[i] = source[i].Guid;
        }
        else
        {
            source = new T[handler.Length];
            int j = 0;
            for (int i = 0; i < source.Length; i++)
            {
                if (IdentifiableDatabase.TryGet(handler[i], out T? identifiable) && identifiable is not null)
                    source[j++] = identifiable;
                else
                    Debug.LogError($"Could not find '{typeof(T)}' with id '{handler}' in the database");
            }

            if (j != source.Length)
                source = source[..j];
        }
    }

    public static void Array<T>(this SavingSystem.Transfer type, ref T[] handler, ref T[] source) where T : unmanaged
    {
        if (type.TryTransferAsNull(ref handler, ref source))
            return;

        if (type == SavingSystem.Transfer.PullFromSource)
        {
            handler = new T[source.Length];
            source.CopyTo(handler.AsSpan());
        }
        else
        {
            source = new T[handler.Length];
            handler.CopyTo(source.AsSpan());
        }
    }

    public static void Array(this SavingSystem.Transfer type, ref string[] handler, ref string[] source)
    {
        if (type.TryTransferAsNull(ref handler, ref source))
            return;

        if (type == SavingSystem.Transfer.PullFromSource)
        {
            handler = new string[source.Length];
            source.CopyTo(handler.AsSpan());
        }
        else
        {
            source = new string[handler.Length];
            handler.CopyTo(source.AsSpan());
        }
    }

    static bool TryTransferAsNull<T, T2>(this SavingSystem.Transfer type, ref T handler, ref T2 source)
    {
        if (type == SavingSystem.Transfer.PullFromSource)
        {
            if (source == null)
            {
                handler = default!;
                return true;
            }
        }
        else
        {
            if (handler == null)
            {
                source = default!;
                return true;
            }
        }

        return false;
    }

    static void EnsureNotNull<T, T2>(this SavingSystem.Transfer type, ref T handler, ref T2 source) where T : new() where T2 : new()
    {
        if (type == SavingSystem.Transfer.PullFromSource)
        {
            handler = new();
        }
        else
        {
            source = new();
        }
    }
}