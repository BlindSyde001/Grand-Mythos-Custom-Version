using System;

[Serializable]
public class FixedEncounter : BaseEncounter
{
    public EnemyExtension[] Formation;

    protected override bool SubIsValid(out string error)
    {
        if (Formation == null)
        {
            error = $"{nameof(Formation)} is null";
            return false;
        }

        error = null;
        return true;
    }

    protected override EnemyExtension[] FormationToSpawn() => Formation;
}