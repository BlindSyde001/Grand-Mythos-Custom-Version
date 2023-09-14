using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class SkillTree : ScriptableObject, ISerializationCallbackReceiver
{
    [InfoBox("Right now this just linearly unlock skills, actual tree is for another milestone")]
    [ValidateInput(nameof(SortValidation))]
    public SkillUnlock[] Skills = Array.Empty<SkillUnlock>();

    public SkillsForLevelEnumerator GetSkillsForLevel(uint level) => new(Skills, level);

    public void Sort()
    {
        // Obviously not optimal but right now optimization given use case is pointless
        Skills = Skills.OrderBy(x => x.Level).ToArray();
    }

    bool SortValidation(SkillUnlock[] val, ref string errorMessage)
    {
        Sort();
        return true;
    }

    public void OnBeforeSerialize(){ }

    public void OnAfterDeserialize() => Sort();

    [Serializable]
    public struct SkillUnlock
    {
        [HorizontalGroup]
        [Required] public Skill Skill;
        [HorizontalGroup]
        public uint Level;
    }

    public struct SkillsForLevelEnumerator : IEnumerator<Skill>, IEnumerable<Skill>
    {
        readonly SkillUnlock[] collection;
        readonly int length;
        readonly uint level;
        int index;

        public Skill Current => collection[index].Skill;
        object IEnumerator.Current => Current;

        public SkillsForLevelEnumerator(SkillUnlock[] arr, uint levelParam)
        {
            collection = arr;
            length = arr.Length;
            index = -1;
            level = levelParam;
        }

        public bool MoveNext()
        {
            index++;
            return index < length && collection[index].Level <= level;
        }

        public void Reset()
        {
            index = -1;
        }

        public void Dispose() {}

        public SkillsForLevelEnumerator GetEnumerator() => this;
        IEnumerator<Skill> IEnumerable<Skill>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}