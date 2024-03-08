public class BattleHeroModelController : BattleCharacterController
{
    string currentAnimState;
    internal bool isPerformingActions;

    const string Battle_EnterFight = "Enter Fight";
    const string Battle_Stance = "Stance";
    const string Battle_Die = "Die";

    // UPDATES
    void OnEnable()
    {
        BattleStateMachine.OnNewStateSwitched += NewCombatState;
    }

    void OnDisable()
    {
        BattleStateMachine.OnNewStateSwitched -= NewCombatState;
    }

    void NewCombatState(CombatState combatState)
    {
        switch(combatState)
        {
            case CombatState.Start:
                if (Profile.CurrentHP > 0)
                {
                    ChangeAnimationState(Battle_EnterFight);
                }
                else
                {
                    ChangeAnimationState(Battle_Die);
                }
                break;

            case CombatState.Active:
                if (Profile.CurrentHP > 0)
                {
                    ChangeAnimationState(Battle_Stance);
                    MovementController.isRoaming = true;
                }
                else
                {
                    MovementController.isRoaming = false;
                }
                break;

            case CombatState.Wait:
                if (Profile.CurrentHP > 0)
                {
                    ChangeAnimationState(Battle_Stance);
                    MovementController.isRoaming = false;
                }
                break;

            case CombatState.End:
                MovementController.isRoaming = false;
                break;
        }
    }

    void ChangeAnimationState(string newAnimState)
    {
        Animator.Play(newAnimState);
        currentAnimState = newAnimState;
    }
}
