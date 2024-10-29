using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class Weapon : Equipment
{
    public enum WeaponType { Gun, Warhammer, PowerGlove, Grimoire};

    [BoxGroup("BASIC DATA")]
    [PropertyOrder(0)]
    public WeaponType weaponType;

    [MaybeNull] public Skill AttachedSkill;
    [Range(0,100),SuffixLabel("%")]
    public float SkillProcChance = 50f;
}