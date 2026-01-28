using System;
using System.Diagnostics.CodeAnalysis;

[Serializable]
public class FixedEncounter : BaseEncounter
{
    public required CharacterTemplate[] Formation;
    public uint Seed = (uint)new Random().Next(int.MinValue, int.MaxValue);

    protected override bool SubIsValid([MaybeNullWhen(true)] out string error)
    {
        if (Formation == null!)
        {
            error = $"{nameof(Formation)} is null";
            return false;
        }

        error = null;
        return true;
    }

    protected override CharacterTemplate[] FormationToSpawn() => Formation;
    protected override uint GetSeedForCharacter(CharacterTemplate character) => Seed;
}