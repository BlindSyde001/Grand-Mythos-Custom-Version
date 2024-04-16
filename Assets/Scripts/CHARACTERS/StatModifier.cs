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

    public void Apply(ref Stats stats)
    {
        foreach (var mod in Mods)
            stats[mod.Attribute] += mod.Value;
    }

    [Serializable]
    public struct Mod
    {
        public Stat Attribute;
        public int Value;
    }
}