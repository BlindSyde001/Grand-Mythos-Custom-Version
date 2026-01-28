using System.Diagnostics.CodeAnalysis;

public interface IEncounterDefinition
{
    Signal<BattleStateMachine> Start(OverworldPlayerController player);
    bool IsValid([MaybeNullWhen(true)] out string error);
}