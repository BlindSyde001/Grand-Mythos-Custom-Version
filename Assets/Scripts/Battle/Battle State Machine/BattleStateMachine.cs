using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Cinemachine;
using Random = UnityEngine.Random;

public class BattleStateMachine : MonoBehaviour
{
    static BattleStateMachine _instance;

    public Team PlayerTeam;
    public SerializableHashSet<CharacterTemplate> Units = new();
    public SerializableHashSet<CharacterTemplate> TacticsDisabled = new();

    [NonSerialized]
    public BlockBattleFlags Blocked;

    List<CharacterTemplate> _unitsCopy = new();
    Dictionary<CharacterTemplate, Tactics> _orders = new();
    [SerializeField] BattleResolution _battleResolution;

    // VARIABLES
    public List<Transform> HeroSpawns;    // Where do they initially spawn?
    public List<Transform> EnemySpawns;

    public delegate void SwitchToNewState(CombatState state);
    public static event SwitchToNewState OnNewStateSwitched;

    CombatState _combatState;
    CinemachineFreeLook _rotateCam;

    public List<HeroExtension> PartyLineup => GameManager._instance.PartyLineup;

    // UPDATES
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(this);
            return;
        }
        _rotateCam = FindObjectOfType<CinemachineFreeLook>();
        InputManager.Instance.PushGameState(GameState.Battle, this);
    }

    void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
        InputManager.Instance.PopGameState(this);
    }

    IEnumerator Start()
    {
        var gameManager = GameManager._instance;
        foreach (var reserve in gameManager.ReservesLineup)
            reserve.gameObject.SetActive(false);

        for (int i = 0; i < gameManager.PartyLineup.Count; i++)
        {
            var hero = gameManager.PartyLineup[i];

            // Add Model into Battle
            var model = Instantiate(hero.BattlePrefab,
                HeroSpawns[i].position,
                HeroSpawns[i].rotation,
                transform.parent);

            model.name = $"{hero.gameObject.name} Model";

            var controller = model.GetComponent<BattleHeroModelController>();
            controller.Template = hero;

            hero.ActionsCharged = Random.Range(0, hero.ActionChargeMax);
        }

        foreach (var target in FindObjectsOfType<CharacterTemplate>())
            Units.Add(target);

        SendStateChangeNotification(CombatState.Start);
        yield return new WaitForSeconds(5);
        SendStateChangeNotification(CombatState.Active);

        do
        {
            if (IsBattleFinished(out bool win))
            {
                _rotateCam.GetComponent<CinemachineFreeLook>().enabled = false;
                SendStateChangeNotification(CombatState.End);
                enabled = false;
                yield return new WaitForSeconds(1f);
                foreach (var yields in _battleResolution.ResolveBattle(win, this))
                    yield return yields;
                yield break;
            }

            if (Blocked == 0)
            {
                _unitsCopy.Clear();
                _unitsCopy.AddRange(Units);
                foreach (var unit in Units)
                {
                    if (unit.CurrentHP == 0 || TacticsDisabled.Contains(unit))
                        continue;

                    // This unit has its ATB full or the manual order can be executed given the current amount of charge
                    if (unit.ActionsCharged >= unit.ActionChargeMax || _orders.TryGetValue(unit, out var chosenTactic) && chosenTactic.Condition.CanExecute(chosenTactic.Actions, new TargetCollection(_unitsCopy), unit.Context, out _, accountForCost:true))
                    {
                        foreach (var yield in CatchException(ProcessUnit(unit)))
                            yield return yield;
                    }
                }

                foreach (var unit in Units)
                {
                    if (unit.CurrentHP != 0)
                        unit.ActionsCharged += unit.ActionRechargeSpeed * Time.deltaTime / 10f;
                    // DO NOT CLAMP HERE, DO IT AFTER ORDERS HAVE BEEN SCHEDULED
                }
            }

            yield return null; // Wait for next frame
        } while (true);
    }

    IEnumerable CatchException(IEnumerable source)
    {
        for (var enumerable = source.GetEnumerator(); ; )
        {
            object yield;
            try
            {
                if (enumerable.MoveNext() == false)
                    break;
                yield = enumerable.Current;
            }
            catch(Exception e)
            {
                Debug.LogException(e);
                break;
            }

            yield return yield;
        }
    }

    bool IsBattleFinished(out bool win)
    {
        int alliesLeft = 0;
        int hostilesLeft = 0;
        foreach (var target in Units)
        {
            if (target.CurrentHP == 0)
                continue;
            if (PlayerTeam.Allies.Contains(target.Team))
                alliesLeft++;
            else
                hostilesLeft++;
        }

        win = hostilesLeft == 0 && alliesLeft > 0;
        return hostilesLeft == 0 || alliesLeft == 0;
    }

    IEnumerable ProcessUnit(CharacterTemplate unit)
    {
        #if UNITY_EDITOR
        // Halting execution to reload assemblies while this enumerator is running
        // may put the state of the combat into an unrecoverable state, make sure that doesn't happen
        UnityEditor.EditorApplication.LockReloadAssemblies();
        #endif
        try
        {
            Tactics chosenTactic;
            TargetCollection selection = default;
            if (_orders.TryGetValue(unit, out chosenTactic) && chosenTactic.Condition.CanExecute(chosenTactic.Actions, new TargetCollection(_unitsCopy), unit.Context, out selection, true))
            {
                // We're taking care of this explicit order
            }
            else
            {
                foreach (var tactic in unit.Tactics)
                {
                    if (tactic.IsOn && tactic.Condition.CanExecute(tactic.Actions, new TargetCollection(_unitsCopy), unit.Context, out selection, true))
                    {
                        chosenTactic = tactic;
                        break;
                    }
                }
            }
            _orders.Remove(unit);

            if (chosenTactic != null)
            {
                var combinedSelection = selection;
                for (int i = 0; i < chosenTactic.Actions.Length; i++)
                {
                    var action = chosenTactic.Actions[i];

                    if (unit.ActionsCharged < action.ATBCost)
                        break;

                    if (i != 0)
                    {
                        // Check that our selection is still valid, may not be after running the previous action
                        var actionLeft = chosenTactic.Actions.AsSpan()[i..];
                        if (chosenTactic.Condition.CanExecute(actionLeft, selection, unit.Context, out _, true) == false)
                        {
                            // If not, check if we have something else to target
                            if (chosenTactic.Condition.CanExecute(actionLeft, new TargetCollection(_unitsCopy), unit.Context, out var newSelection, true))
                            {
                                // Continue with that selection instead
                                selection = newSelection;
                                combinedSelection |= newSelection;
                            }
                            else
                                break;
                        }
                    }

                    unit.ActionsCharged -= action.ATBCost;

                    foreach (var yield in action.Perform(selection, unit.Context))
                        yield return yield;

                    if (Units.Contains(unit) == false)
                        break;
                }

                chosenTactic.Condition.TargetFilter?.NotifyUsedCondition(combinedSelection, unit.Context);
                chosenTactic.Condition.AdditionalCondition?.NotifyUsedCondition(combinedSelection, unit.Context);
                foreach (var action in chosenTactic.Actions)
                {
                    action.TargetFilter?.NotifyUsedCondition(combinedSelection, unit.Context);
                    action.Precondition?.NotifyUsedCondition(combinedSelection, unit.Context);
                }

                unit.Context.Round++;
            }

            if (unit.ActionsCharged > unit.ActionChargeMax)
                unit.ActionsCharged = unit.ActionChargeMax;
        }
        finally
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.UnlockReloadAssemblies();
#endif
        }
    }

    public static bool TryGetInstance([MaybeNullWhen(false)] out BattleStateMachine bts)
    {
        if (_instance != null)
        {
            bts = _instance;
            return true;
        }

        bts = null;
        return false;
    }

    public void Include(CharacterTemplate unit)
    {
        if (unit == null)
            throw new NullReferenceException(nameof(unit));
        Units.Add(unit);
    }

    public void Exclude(CharacterTemplate unit)
    {
        if (unit == null)
            return;
        Units.Remove(unit);
    }

    public void SetOrderFor(CharacterTemplate character, Tactics specificTactic) => _orders[character] = specificTactic;

    // METHODS
    void SendStateChangeNotification(CombatState newCombatState)
    {
        try
        {
            switch(newCombatState)
            {
                case CombatState.Start:
                    _combatState = CombatState.Start;
                    OnNewStateSwitched?.Invoke(newCombatState);
                    break;

                case CombatState.Active:
                    _combatState = CombatState.Active;
                    OnNewStateSwitched?.Invoke(newCombatState);
                    break;

                case CombatState.Wait:
                    _combatState = CombatState.Wait;
                    OnNewStateSwitched?.Invoke(newCombatState);
                    break;

                case CombatState.End:
                    _combatState = CombatState.End;
                    OnNewStateSwitched?.Invoke(newCombatState);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newCombatState), newCombatState, null);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    #region End of Battle
    internal static void ClearData()
    {
        #warning clean this stuff up
        foreach (var hostile in FindObjectsOfType<BattleCharacterController>())
        {
            if (hostile.Template != null && hostile.Template is not HeroExtension)
                Destroy(hostile.Template);
            Destroy(hostile.gameObject);
        }
    }
    #endregion
}


[Flags]
public enum BlockBattleFlags
{
    PreparingOrders = 0b0001,
}