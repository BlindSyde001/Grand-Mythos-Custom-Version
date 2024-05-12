using System;

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

    [Serializable]
    public struct Mod
    {
        public Stat Attribute;
        public int Value;
    }
}