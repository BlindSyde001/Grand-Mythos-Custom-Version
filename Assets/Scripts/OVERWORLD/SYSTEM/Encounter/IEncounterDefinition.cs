using UnityEngine;

public interface IEncounterDefinition
{
    void Start(Transform hintSource, OverworldPlayerControlsNode player);
    bool IsValid(out string error);
}