using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class TacticsMenuActions : MenuContainer
{
    public GameObject SegmentsWarning;

    public UIElementList<Button> HeroSelections = new();
    public List<Button> PageList;
    public List<TacticsModuleContainer> TacticsModules;
    public List<NewComponentContainer> NewComponentList;

    Button _dropdownSource;

    readonly List<IAction> _actionsList = new();

    //METHODS
    public override IEnumerable Open(MenuInputs menuInputs)
    {
        SetHeroSelection();
        SetTacticsList(GameManager.PartyLineup[0]);
        gameObject.SetActive(true);
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-800, 480, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(500, 470, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(230, -100, 0), menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
    }
    public override IEnumerable Close(MenuInputs menuInputs)
    {
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-1350, 480, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(500, 610, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(1700, -100, 0), menuInputs.Speed);
        gameObject.transform.GetChild(3).DOLocalMove(new Vector3(-1300, 328, 0), menuInputs.Speed);
        gameObject.transform.GetChild(4).DOLocalMove(new Vector3(-1300, -100, 0), menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
        gameObject.SetActive(false);
    }

    IEnumerator ComponentListOpen()
    {
        gameObject.transform.GetChild(3).gameObject.SetActive(true);
        gameObject.transform.GetChild(4).gameObject.SetActive(true);
        gameObject.transform.GetChild(3).DOLocalMove(new Vector3(-740, 328, 0), MenuInputs.Speed);
        gameObject.transform.GetChild(4).DOLocalMove(new Vector3(-710, -100, 0), MenuInputs.Speed);
        yield return new WaitForSeconds(MenuInputs.Speed);
    }

    IEnumerator ComponentListClose()
    {
        gameObject.transform.GetChild(3).DOLocalMove(new Vector3(-1300, 328, 0), MenuInputs.Speed);
        gameObject.transform.GetChild(4).DOLocalMove(new Vector3(-1300, -100, 0), MenuInputs.Speed);
        yield return new WaitForSeconds(MenuInputs.Speed);
        gameObject.transform.GetChild(3).gameObject.SetActive(false);
        gameObject.transform.GetChild(4).gameObject.SetActive(false);
    }



    internal void SetHeroSelection()
    {
        HeroSelections.Clear();
        foreach (var hero in GameManager.PartyLineup)
        {
            HeroSelections.Allocate(out var element);
            element.GetComponent<Image>().sprite = hero.Portrait;
            element.onClick.AddListener(delegate { SetTacticsList(hero); });
        }
    }
    public void SetTacticsList(HeroExtension hero)
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine(ComponentListClose());
        ResetActionSegments();
        // Configure all the TacticsList Buttons: On/Off, Select Cnd, Select Action
        for (int i = 0; i < TacticsModules.Count; i++)
        {
            int j = i;
            var heroTactic = i < hero.Tactics.Length ? hero.Tactics[i] : null;
            TacticsModules[i].onToggle.text = heroTactic == null ? "Off" : heroTactic.IsOn ? "On" : "Off";
            TacticsModules[i].condition.text = heroTactic?.Condition != null ? heroTactic.Condition.name : "";

            // Go through all my Actions and turn on their respective Buttons (Based on Segment Cost)
            uint costTotal = 0;
            for (int actionIndex = 0; actionIndex < heroTactic?.Actions.Length; actionIndex++)
            {
                SetActionsBasedOnSegmentCost(hero, heroTactic.Actions[actionIndex] != null ? heroTactic.Actions[actionIndex].ActionCost : 0, i, actionIndex, costTotal, OnSetAction);
                costTotal += heroTactic.Actions[actionIndex] != null ? heroTactic.Actions[actionIndex].ActionCost : 0;
            }

            foreach(Button btn in TacticsModules[i].addActionBtns)
                btn.gameObject.SetActive(false);

            if (costTotal < 4)
            {
                var btn = TacticsModules[i].addActionBtns[(int)costTotal];
                btn.gameObject.SetActive(true);
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    if (j >= hero.Tactics.Length)
                    {
                        var newTactics = new Tactics[j+1];
                        hero.Tactics.CopyTo(newTactics, 0);
                        hero.Tactics = newTactics;
                    }

                    hero.Tactics[j] ??= new(){ IsOn = false };
                    int slot = hero.Tactics[j].Actions.Length;
                    ShowActionDropdown(hero, action => OnSetAction(action, slot), btn);
                });
            }

            TacticsModules[i].onToggleBtn.interactable = heroTactic != null && heroTactic.Condition != null;
            TacticsModules[i].onToggleBtn.onClick.RemoveAllListeners();
            TacticsModules[i].onToggleBtn.onClick.AddListener(OnToggleTacticPressed);

            TacticsModules[i].conditionBtn.onClick.RemoveAllListeners();
            TacticsModules[i].conditionBtn.onClick.AddListener(() => ShowConditionDropdown(TacticsModules[j], OnSetCondition, TacticsModules[j].conditionBtn));

            void OnSetAction(IAction newAction, int actionIndex)
            {
                if (newAction is null)
                    throw new NullReferenceException(nameof(newAction));

                if (j >= hero.Tactics.Length)
                {
                    var newTactics = new Tactics[j + 1];
                    hero.Tactics.CopyTo(newTactics, 0);
                    hero.Tactics = newTactics;
                }
                hero.Tactics[j] ??= new() { IsOn = false };

                var tactic = hero.Tactics[j];
                if (actionIndex >= tactic.Actions.Length) // Make sure we have space for this new action
                {
                    IActionCollection newCollection = new()
                    {
                        BackingArray = new IAction[actionIndex+1]
                    };
                    tactic.Actions.BackingArray.CopyTo(newCollection.BackingArray, 0);
                    tactic.Actions = newCollection;
                }

                tactic.Actions[actionIndex] = newAction; // Insert this new action

                // Remove any actions which would go over our max amount of charge
                var newActions = new List<IAction>();
                uint totalCost = 0;
                foreach (var action in tactic.Actions)
                {
                    if (totalCost + action.ActionCost > hero.ActionChargeMax)
                        break;
                    newActions.Add(action);
                }

                tactic.Actions = new(){ BackingArray = newActions.ToArray() };
                if (tactic.Condition is null || tactic.Actions.Length == 0)
                    tactic.IsOn = false;
            }

            void OnToggleTacticPressed()
            {
                if (j >= hero.Tactics.Length)
                    return;

                var thisContainer = TacticsModules[j];
                var tactic = hero.Tactics[j];
                if(tactic?.Condition == null || tactic.Actions.Length == 0)
                {
                    if (tactic is not null)
                        tactic.IsOn = false;
                    thisContainer.onToggle.text = "Off";
                    return;
                }
                tactic.IsOn = !tactic.IsOn;
                thisContainer.onToggle.text = tactic.IsOn ? "On" : "Off";
            }

            void OnSetCondition(ActionCondition newCondition)
            {
                if (j >= hero.Tactics.Length)
                {
                    var newTactics = new Tactics[j + 1];
                    hero.Tactics.CopyTo(newTactics, 0);
                    hero.Tactics = newTactics;
                }

                hero.Tactics[j] ??= new() { IsOn = false };
                hero.Tactics[j].Condition = newCondition;
                if (hero.Tactics[j].Condition is null || hero.Tactics[j].Actions.Length == 0)
                    hero.Tactics[j].IsOn = false;
            }
        }
    }
    #region Action Segment Methods

    void ResetActionSegments()
    {
        foreach(TacticsModuleContainer t in TacticsModules)
        {
            foreach(Button a in t.singleActionBtns)
                a.gameObject.SetActive(false);
            foreach (Button a in t.doubleActionBtns)
                a.gameObject.SetActive(false);
            foreach (Button a in t.tripleActionBtns)
                a.gameObject.SetActive(false);
            t.quadActionBtn.gameObject.SetActive(false);

            for(int i = 0; i < t.addActionBtns.Count; i++)
            {
                t.addActionBtns[i].gameObject.SetActive(i == 0);
            }
        }
    }

    void SetActionsBasedOnSegmentCost(HeroExtension selectedHero, uint actionCost, int tactic, int action, uint previousActionCostTotal, Action<IAction, int> OnNewAction)
    {
        if (previousActionCostTotal + actionCost > 4)
        {
            Debug.Log("Not enough Segments!");
            ReOrderActions(selectedHero, tactic);
            return;
        }

        Button actionSelection;
        TextMeshProUGUI textProvider;
        switch (actionCost)
        {
            case 1:
                actionSelection = TacticsModules[tactic].singleActionBtns[(int)previousActionCostTotal];
                textProvider = TacticsModules[tactic].singlesText[(int)previousActionCostTotal];
                break;

            case 2:
                actionSelection = TacticsModules[tactic].doubleActionBtns[(int)previousActionCostTotal];
                textProvider = TacticsModules[tactic].doublesText[(int)previousActionCostTotal];
                break;

            case 3:
                actionSelection = TacticsModules[tactic].tripleActionBtns[(int)previousActionCostTotal];
                textProvider = TacticsModules[tactic].triplesText[(int)previousActionCostTotal];
                break;

            case 4:
                actionSelection = TacticsModules[tactic].quadActionBtn;
                textProvider = TacticsModules[tactic].quadruplesText;
                break;

            default:
                throw new NotImplementedException();
        }

        textProvider.text = selectedHero.Tactics[tactic].Actions[action].Name;

        actionSelection.gameObject.SetActive(true);
        actionSelection.onClick.RemoveAllListeners();
        actionSelection.onClick.AddListener(() => ShowActionDropdown(selectedHero, actionObject => OnNewAction(actionObject, action), actionSelection));
    }

    IEnumerator SendWarning()
    {
        SegmentsWarning.SetActive(true);
        yield return new WaitForSeconds(2f);
        SegmentsWarning.SetActive(false);
    }

    void ReOrderActions(HeroExtension selectedHero, int tOrder)
    {
        uint eAllowance = 0;
        for (int i = 0; i < selectedHero.Tactics[tOrder].Actions.Length; i++)
        {
            if (selectedHero.Tactics[tOrder].Actions[i] != null)
            {
                if (eAllowance + selectedHero.Tactics[tOrder].Actions[i].ActionCost > 4)
                {
                    StartCoroutine(SendWarning());
                    selectedHero.Tactics[tOrder].Actions[i] = null;
                }
                else
                {
                    eAllowance += selectedHero.Tactics[tOrder].Actions[i].ActionCost;
                }
            }
        }
    }
    #endregion
    #region Open Page of Conditions/Actions
    public void SetPagesOnClickAction(Action<int> OnClick)
    {
        for (int i = 0; i < PageList.Count; i++)
        {
            int j = i;
            PageList[i].onClick.RemoveAllListeners();
            PageList[i].onClick.AddListener(() => OnClick(j));
        }
    }

    /// <summary>
    /// Change Cnds represented on the CndList to match the Page Number
    /// </summary>
    public void PopulateConditionList(TacticsModuleContainer currentContainer, int pageNo, Action<ActionCondition> OnConditionSelected)
    {
        foreach (var component in NewComponentList)
            component.cmpButton.onClick.RemoveAllListeners();

        bool setSelection = false;
        for (int i = 0; i < NewComponentList.Count; i++) // iterate through the Buttons
        {
            NewComponentList[i].cmpName.text = "";
            if (InventoryManager.ConditionsAcquired.Count <= (pageNo * 10) + i) // Check if there's a cnd in that Slot
                continue;

            if (!InventoryManager.ConditionsAcquired.Contains(InventoryManager.ConditionsAcquired[(pageNo * 10) + i])) // Check if you unlocked that Cnd
                continue;

            NewComponentList[i].selectedCnd = InventoryManager.ConditionsAcquired[(pageNo * 10) + i];
            NewComponentList[i].cmpName.text = NewComponentList[i].selectedCnd.name;

            int j = i;
            NewComponentList[i].cmpButton.onClick.RemoveAllListeners();
            NewComponentList[i].cmpButton.onClick.AddListener(() =>
            {
                var newCondition = NewComponentList[j].selectedCnd;
                OnConditionSelected(newCondition);
                currentContainer.condition.text = newCondition.name;
                StartCoroutine(ComponentListClose());
            });
            if (setSelection == false && NewComponentList[i].cmpButton.interactable)
            {
                setSelection = true;
                NewComponentList[i].cmpButton.Select();
            }
        }
    }

    public void PopulateActionList(HeroExtension selectedHero, int pageNo, Action<IAction> OnNewAction)
    {
        _actionsList.Clear();
        _actionsList.Add(selectedHero.BasicAttack);
        foreach(var (item, _) in InventoryManager.Enumerate<Consumable>())
            _actionsList.Add(item);
        foreach(var a in selectedHero.Skills)
            _actionsList.Add(a);

        foreach (var component in NewComponentList)
            component.cmpButton.onClick.RemoveAllListeners();

        bool setSelection = false;
        for (int i = 0; i < NewComponentList.Count; i++) // iterate through the Buttons
        {
            NewComponentList[i].cmpName.text = "";
            if (_actionsList.Count <= (pageNo * 10) + i) // Set new Page up
                continue;

            if (_actionsList[(pageNo * 10) + i] is Skill skill
                && (selectedHero.Skills.Contains(skill) || selectedHero.BasicAttack == skill))
            {
                NewComponentList[i].selectedAction = _actionsList[(pageNo * 10) + i];
                NewComponentList[i].cmpName.text = NewComponentList[i].selectedAction.Name;

                int j = i;
                NewComponentList[i].cmpButton.onClick.RemoveAllListeners();
                NewComponentList[i].cmpButton.onClick.AddListener(() => SelectNewListAction(selectedHero, NewComponentList[j].selectedAction, OnNewAction));
                if (setSelection == false && NewComponentList[i].cmpButton.interactable)
                {
                    setSelection = true;
                    NewComponentList[i].cmpButton.Select();
                }
            }
            else
            {
                foreach(var (item, _) in InventoryManager.Enumerate<Consumable>())
                {
                    if (item.name != _actionsList[(pageNo * 10) + i].Name)
                        continue;

                    NewComponentList[i].selectedAction = _actionsList[(pageNo * 10) + i];
                    NewComponentList[i].cmpName.text = NewComponentList[i].selectedAction.Name;

                    int j = i;
                    NewComponentList[i].cmpButton.onClick.RemoveAllListeners();
                    NewComponentList[i].cmpButton.onClick.AddListener(() => SelectNewListAction(selectedHero, NewComponentList[j].selectedAction, OnNewAction));
                    if (setSelection == false && NewComponentList[i].cmpButton.interactable)
                    {
                        setSelection = true;
                        NewComponentList[i].cmpButton.Select();
                    }
                }
            }
        }
    }
    #endregion

    #region Swapping Conditions
    public void ShowConditionDropdown(TacticsModuleContainer currentContainer, Action<ActionCondition> OnConditionSelected, Button dropdownSource)
    {
        if (_dropdownSource != dropdownSource)
        {
            StartCoroutine(ComponentListOpen());
            SetPagesOnClickAction(j => PopulateConditionList(currentContainer, j, OnConditionSelected));
            PopulateConditionList(currentContainer, 0, OnConditionSelected);
        }
        else // Close if the user pressed on the same source
        {
            StartCoroutine(ComponentListClose());
        }
    }

    #endregion
    #region Swapping Actions
    public void ShowActionDropdown(HeroExtension selectedHero, Action<IAction> OnClick, Button dropdownSource)
    {
        if (_dropdownSource != dropdownSource)
        {
            StartCoroutine(ComponentListOpen());
            SetPagesOnClickAction(j => PopulateActionList(selectedHero, j, OnClick));
            PopulateActionList(selectedHero, 0, OnClick);
        }
        else // Close if the user pressed on the same source
        {
            StartCoroutine(ComponentListClose());
        }
    }

    public void SelectNewListAction(HeroExtension selectedHero, IAction action, Action<IAction> OnNewAction)
    {
        OnNewAction(action);

        // Swap Action UI
        SetTacticsList(selectedHero);
        StartCoroutine(ComponentListClose());
    }

    public void CloseDropdown()
    {
        StartCoroutine(ComponentListClose());
    }

    #endregion
}
