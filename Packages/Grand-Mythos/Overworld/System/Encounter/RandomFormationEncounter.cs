using System;
using System.Diagnostics.CodeAnalysis;
using Random = UnityEngine.Random;

[Serializable]
public class RandomFormationEncounter : BaseEncounter
{
    public EncounterAndRate[] RandomFormation = Array.Empty<EncounterAndRate>();
    public uint Seed = (uint)Random.Range(int.MinValue, int.MaxValue);

    protected override bool SubIsValid([MaybeNullWhen(true)] out string error)
    {
        for (int i = 0; i < RandomFormation.Length; i++)
        {
            EncounterAndRate encounterAndRate = RandomFormation[i];
            if (encounterAndRate.Formation.Length == 0)
            {
                error = $"{nameof(RandomFormation)} #{i} is empty";
                return false;
            }

            for (int j = 0; j < encounterAndRate.Formation.Length; j++)
            {
                if (encounterAndRate.Formation[j] == null!)
                {
                    error = $"Opponent #{j} in {nameof(RandomFormation)} #{i} is null";
                    return false;
                }
            }

            if (encounterAndRate.Chance > int.MaxValue)
            {
                error = $"{nameof(encounterAndRate.Chance)} #{i} is too large";
                return false;
            }
        }

        error = null;
        return true;
    }

    protected override CharacterTemplate[] FormationToSpawn()
    {
        uint total = 0;

        for (int i = 0; i < RandomFormation.Length; i++)
            total += RandomFormation[i].Chance;

        if (total == 0)
            return RandomFormation[Random.Range(0, RandomFormation.Length)].Formation;

        total = (uint)Random.Range(0, (int)total);

        for (int i = 0; i < RandomFormation.Length; i++)
        {
            if (total < RandomFormation[i].Chance)
                return RandomFormation[i].Formation;

            total -= RandomFormation[i].Chance;
        }

        return RandomFormation[0].Formation;
    }

    protected override uint GetSeedForCharacter(CharacterTemplate character) => Seed;

    [Serializable]
    public struct EncounterAndRate
    {
        public required CharacterTemplate[] Formation;
        public required uint Chance;
    }
}