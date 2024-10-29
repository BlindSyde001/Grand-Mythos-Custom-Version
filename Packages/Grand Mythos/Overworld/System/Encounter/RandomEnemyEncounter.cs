using System;
using Random = UnityEngine.Random;

[Serializable]
public class RandomEnemyEncounter : BaseEncounter
{
    public EncounterAndRate[] RandomUnits = Array.Empty<EncounterAndRate>();
    public RangeInt FormationSize = new RangeInt(1, 3);
    public uint Seed = (uint)Random.Range(int.MinValue, int.MaxValue);

    protected override bool SubIsValid(out string error)
    {
        if (FormationSize.Min <= 0)
        {
            error = $"{nameof(FormationSize)} cannot be zero, a formation should have at least one unit";
            return false;
        }

        for (int i = 0; i < RandomUnits.Length; i++)
        {
            EncounterAndRate encounterAndRate = RandomUnits[i];
            if (encounterAndRate.Unit == null)
            {
                error = $"{nameof(RandomUnits)} #{i} is null";
                return false;
            }
            if (encounterAndRate.Chance > int.MaxValue)
            {
                error = $"{nameof(encounterAndRate.Chance)} #{i} is too large";
                return false;
            }
        }

        error = null;
        return false;
    }

    protected override CharacterTemplate[] FormationToSpawn()
    {
        int formationSize = Random.Range(FormationSize.Min, FormationSize.Max+1);
        var output = new CharacterTemplate[formationSize];
        for (int j = 0; j < formationSize; j++)
        {
            uint total = 0;

            for (int i = 0; i < RandomUnits.Length; i++)
                total += RandomUnits[i].Chance;

            if (total == 0)
            {
                output[j] = RandomUnits[Random.Range(0, RandomUnits.Length)].Unit;
                continue;
            }

            total = (uint)Random.Range(0, (int)total);

            for (int i = 0; i < RandomUnits.Length; i++)
            {
                if (total < RandomUnits[i].Chance)
                {
                    output[j] = RandomUnits[i].Unit;
                    break;
                }

                total -= RandomUnits[i].Chance;
            }

            if (output[j] == null)
                output[j] = RandomUnits[0].Unit;
        }

        return output;
    }

    protected override uint GetSeedForCharacter(CharacterTemplate character) => Seed;

    [Serializable]
    public struct EncounterAndRate
    {
        public CharacterTemplate Unit;
        public uint Chance;
    }
}