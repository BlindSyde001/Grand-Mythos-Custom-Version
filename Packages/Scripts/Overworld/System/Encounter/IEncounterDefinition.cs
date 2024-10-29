using UnityEngine;

public interface IEncounterDefinition
{
    void Start(Transform hintSource, OverworldPlayerController player);
    bool IsValid(out string error);
}