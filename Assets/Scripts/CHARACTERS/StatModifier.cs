using System;
using Characters;

[Serializable]
public class StatModifier : IModifier
{
    public Mod[] Mods =
    {
        new()
        {
            Attribute = Stat.Health,
            Value = 10,
        }
    };

    public ModifierDisplay DisplayPrefab => null;

    public void ModifyStats(ref Stats stats)
    {
        foreach (var mod in Mods)
            stats[mod.Attribute] += mod.Value;
    }

    public void ModifyOutgoingDelta(EvaluationContext context, BattleCharacterController target, ref ComputableDamageScaling scaling)
    {

    }

    public void ModifyIncomingDelta(EvaluationContext context, BattleCharacterController target, ref ComputableDamageScaling scaling)
    {

    }

    public bool Temporary => false;
    public bool IsStillValid(AppliedModifier data, EvaluationContext context) => true;

    [Serializable]
    public struct Mod
    {
        public Stat Attribute;
        public int Value;
    }
}