using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TargetCollection : IEnumerable<CharacterTemplate>
{
    List<CharacterTemplate> _targets;
    ulong _included;

    public bool IsEmpty => _included == 0;

    public TargetCollection(List<CharacterTemplate> targets)
    {
        Debug.Assert(targets.Count <= sizeof(ulong));
        // Support for more than 64 targets is feasible, just not something I think we need to focus on right now
        if (targets.Count == sizeof(ulong) * 8)
            _included = ~0ul; // Set all bits
        else
            _included = (1ul << targets.Count) - 1ul; // Set the first x bits on, for three targets, return 0b0111
        _targets = targets;
    }

    public readonly bool TryGetNext(ref int i, out CharacterTemplate target)
    {
        i += 1;
        for (; i < _targets.Count; i++)
        {
            if (((1ul << i) & _included) != 0)
            {
                target = _targets[i];
                return true;
            }
        }

        target = null;
        return false;
    }

    public void RemoveAt(int index)
    {
        _included &= ~(1ul << index);
    }

    public void Empty()
    {
        _included = 0;
    }

    public readonly int CountSlow()
    {
        int total = 0;
        for (int i = 0; i < _targets.Count; i++)
        {
            if (((1ul << i) & _included) != 0)
                total++;
        }

        return total;
    }

    public readonly CharacterTemplate[] ToArray()
    {
        CharacterTemplate[] array = new CharacterTemplate[CountSlow()];
        for (int i = -1, c = 0; TryGetNext(ref i, out var target); c++)
            array[c] = target;

        return array;
    }

    public static TargetCollection operator &(in TargetCollection a, in TargetCollection b)
    {
        Debug.Assert(ReferenceEquals(a._targets, b._targets));
        TargetCollection output;
        output._targets = a._targets;
        output._included = a._included & b._included;
        return output;
    }

    public static TargetCollection operator |(in TargetCollection a, in TargetCollection b)
    {
        Debug.Assert(ReferenceEquals(a._targets, b._targets));
        TargetCollection output;
        output._targets = a._targets;
        output._included = a._included | b._included;
        return output;
    }

    public Enum GetEnumerator() => new Enum(_targets, _included);
    IEnumerator<CharacterTemplate> IEnumerable<CharacterTemplate>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enum : IEnumerator<CharacterTemplate>
    {
        List<CharacterTemplate> _targets;
        ulong _included;
        int _i;

        public CharacterTemplate Current { get; private set; }

        object IEnumerator.Current => Current;

        public Enum(List<CharacterTemplate> targets, ulong included)
        {
            _targets = targets;
            _included = included;
            _i = -1;
            Current = null;
        }

        public bool MoveNext()
        {
            _i += 1;
            for (; _i < _targets.Count; _i++)
            {
                if (((1ul << _i) & _included) != 0)
                {
                    Current = _targets[_i];
                    return true;
                }
            }

            Current = null;
            return false;
        }

        public void Reset()
        {
            _i = -1;
        }

        public void Dispose(){ }
    }
}