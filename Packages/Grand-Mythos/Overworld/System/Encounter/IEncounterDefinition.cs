public interface IEncounterDefinition
{
    Signal<BattleStateMachine> Start(OverworldPlayerController player);
    bool IsValid(out string error);
}