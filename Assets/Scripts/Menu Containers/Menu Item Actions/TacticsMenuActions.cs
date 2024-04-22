using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.InputSystem;

public class TacticsMenuActions : MenuContainer
{
    public GameObject SegmentsWarning;

    public UIElementList<Button> HeroSelections = new();
    public List<Button> PageList;
    public List<TacticsModuleContainer> TacticsModules;
    public List<NewComponentContainer> NewComponentList;
    [Required] public RectTransform NewComponentParentRect;
    [Required] public InputActionReference SwitchHero;
    [Required] public InputActionReference SwitchPage;
    HeroExtension _selectedHero;
    int _currentPage;

    Button _dropdownSource;

    readonly List<IAction> _actionsList = new();

    //METHODS
    public override IEnumerable Open(MenuInputs menuInputs)
    {
        HeroSelections.Clear();
        foreach (var hero in GameManager.PartyLineup)
        {
            HeroSelections.Allocate(out var element);
            element.GetComponent<Image>().sprite = hero.Portrait;
            element.onClick.AddListener(() => ChangeCharacter(hero) );
        }

        ChangeCharacter(GameManager.PartyLineup[0]);
        gameObject.SetActive(true);
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(500, 470, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(230, -100, 0), menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
        SwitchHero.action.performed += Switch;
    }
    public override IEnumerable Close(MenuInputs menuInputs)
    {
        SwitchHero.action.performed -= Switch;
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(500, 610, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(1700, -100, 0), menuInputs.Speed);
        NewComponentParentRect.DOLocalMove(new Vector3(-1300, -100, 0), menuInputs.Speed);
        SwitchPage.action.performed -= SwitchPagePerformed;

        yield return new WaitForSeconds(menuInputs.Speed);

        NewComponentParentRect.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    void Switch(InputAction.CallbackContext input)
    {
        int indexOf = GameManager.PartyLineup.IndexOf(_selectedHero);
        indexOf += input.ReadValue<float>() >= 0f ? 1 : -1;
        indexOf = indexOf < 0 ? GameManager.PartyLineup.Count + indexOf : indexOf % GameManager.PartyLineup.Count;

        ChangeCharacter(GameManager.PartyLineup[indexOf]);
    }

    IEnumerator ComponentListOpen()
    {
        NewComponentParentRect.gameObject.SetActive(true);
        NewComponentParentRect.DOLocalMove(new Vector3(-710, -100, 0), MenuInputs.Speed);
        yield return new WaitForSeconds(MenuInputs.Speed);
        SwitchPage.action.performed += SwitchPagePerformed;
    }

    void SwitchPagePerformed(InputAction.CallbackContext input)
    {
        int indexOf = _currentPage;
        indexOf += input.ReadValue<float>() >= 0f ? 1 : -1;
        indexOf = indexOf < 0 ? PageList.Count + indexOf : indexOf % PageList.Count;

        PageList[indexOf].onClick.Invoke();
    }

    IEnumerator ComponentListClose()
    {
        SwitchPage.action.performed -= SwitchPagePerformed;
        NewComponentParentRect.DOLocalMove(new Vector3(-1300, -100, 0), MenuInputs.Speed);
        yield return new WaitForSeconds(MenuInputs.Speed);
        NewComponentParentRect.gameObject.SetActive(false);
    }

    public void ChangeCharacter(HeroExtension hero)
    {
        _selectedHero = hero;
        if (gameObject.activeInHierarchy)
            StartCoroutine(ComponentListClose());
        // Configure all the TacticsList Buttons: On/Off, Select Cnd, Select Action
        for (int i = 0; i < TacticsModules.Count; i++)
            SetupField(i);
    }

    void SetupField(int i)
    {
        TacticsModuleContainer tacticsUI = TacticsModules[i];
        foreach(var button in tacticsUI.singleActionBtns)
            button.gameObject.SetActive(false);
        foreach (var button in tacticsUI.doubleActionBtns)
            button.gameObject.SetActive(false);
        foreach (var button in tacticsUI.tripleActionBtns)
            button.gameObject.SetActive(false);
        tacticsUI.quadActionBtn.gameObject.SetActive(false);

        for(int j = 0; j < tacticsUI.addActionBtns.Count; j++)
            tacticsUI.addActionBtns[j].gameObject.SetActive(j == 0);

        var heroTactic = i < _selectedHero.Tactics.Length ? _selectedHero.Tactics[i] : null;

        // Go through all my Actions and turn on their respective Buttons (Based on Segment Cost)
        uint costTotal = 0;
        for (int actionIndex = 0; actionIndex < heroTactic?.Actions.Length; actionIndex++)
        {
            SetActionsBasedOnSegmentCost(_selectedHero, heroTactic.Actions[actionIndex] != null ? heroTactic.Actions[actionIndex].ActionCost : 0, i, actionIndex, costTotal, OnSetAction);
            costTotal += heroTactic.Actions[actionIndex] != null ? heroTactic.Actions[actionIndex].ActionCost : 0;
        }

        foreach(Button btn in tacticsUI.addActionBtns)
            btn.gameObject.SetActive(false);

        if (costTotal < 4)
        {
            var btn = tacticsUI.addActionBtns[(int)costTotal];
            btn.gameObject.SetActive(true);
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                if (i >= _selectedHero.Tactics.Length)
                {
                    var newTactics = new Tactics[i+1];
                    _selectedHero.Tactics.CopyTo(newTactics, 0);
                    _selectedHero.Tactics = newTactics;
                }

                _selectedHero.Tactics[i] ??= new(){ IsOn = false };
                int slot = _selectedHero.Tactics[i].Actions.Length;
                ShowActionDropdown(_selectedHero, action => OnSetAction(action, slot), btn);
            });
        }

        tacticsUI.onToggle.text = heroTactic == null ? "Off" : heroTactic.IsOn ? "On" : "Off";
        tacticsUI.condition.text = heroTactic?.Condition != null ? heroTactic.Condition.name : "";
        tacticsUI.onToggleBtn.interactable = heroTactic != null && heroTactic.Condition != null && heroTactic.Actions.Length > 0;
        tacticsUI.onToggleBtn.onClick.RemoveAllListeners();
        tacticsUI.onToggleBtn.onClick.AddListener(OnToggleTacticPressed);

        tacticsUI.conditionBtn.onClick.RemoveAllListeners();
        tacticsUI.conditionBtn.onClick.AddListener(() => ShowConditionDropdown(OnSetCondition, tacticsUI.conditionBtn));

        void OnSetAction(IAction newAction, int actionIndex)
        {
            if (newAction is null)
                throw new NullReferenceException(nameof(newAction));

            if (i >= _selectedHero.Tactics.Length)
            {
                var newTactics = new Tactics[i + 1];
                _selectedHero.Tactics.CopyTo(newTactics, 0);
                _selectedHero.Tactics = newTactics;
            }
            _selectedHero.Tactics[i] ??= new() { IsOn = false };

            var tactic = _selectedHero.Tactics[i];
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
                if (totalCost + action.ActionCost > _selectedHero.ActionChargeMax)
                    break;
                newActions.Add(action);
            }

            tactic.Actions = new(){ BackingArray = newActions.ToArray() };
            SetupField(i);
        }

        void OnToggleTacticPressed()
        {
            if (i >= _selectedHero.Tactics.Length)
                return;

            var tactic = _selectedHero.Tactics[i];
            if (tactic is not null && tactic.Condition != null && tactic.Actions.Length > 0)
                tactic.IsOn = !tactic.IsOn;
            SetupField(i);
        }

        void OnSetCondition(ActionCondition newCondition)
        {
            if (i >= _selectedHero.Tactics.Length)
            {
                var newTactics = new Tactics[i + 1];
                _selectedHero.Tactics.CopyTo(newTactics, 0);
                _selectedHero.Tactics = newTactics;
            }

            var tactic = _selectedHero.Tactics[i] ??= new() { IsOn = false };
            tactic.Condition = newCondition;
            SetupField(i);
        }
    }

    #region Action Segment Methods

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

        textProvider.text = selectedHero.Tactics[tactic]?.Actions[action].Name ?? "";

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
        var tactic = selectedHero.Tactics[tOrder];
        if (tactic == null)
            return;

        for (int i = 0; i < tactic.Actions.Length; i++)
        {
            if (tactic.Actions[i] != null)
            {
                if (eAllowance + tactic.Actions[i].ActionCost > 4)
                {
                    StartCoroutine(SendWarning());
                    tactic.Actions[i] = null;
                }
                else
                {
                    eAllowance += tactic.Actions[i].ActionCost;
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
            PageList[i].onClick.AddListener(() => _currentPage = j);
            PageList[i].onClick.AddListener(() => OnClick(j));
        }
    }

    /// <summary>
    /// Change Cnds represented on the CndList to match the Page Number
    /// </summary>
    public void PopulateConditionList(int pageNo, Action<ActionCondition> OnConditionSelected)
    {
        foreach (var component in NewComponentList)
            component.cmpButton.onClick.RemoveAllListeners();

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
                StartCoroutine(ComponentListClose());
            });
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
    public void ShowConditionDropdown(Action<ActionCondition> OnConditionSelected, Button dropdownSource)
    {
        if (_dropdownSource != dropdownSource)
        {
            StartCoroutine(ComponentListOpen());
            SetPagesOnClickAction(j => PopulateConditionList(j, OnConditionSelected));
            PopulateConditionList(0, OnConditionSelected);
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
        StartCoroutine(ComponentListClose());
    }

    public void CloseDropdown()
    {
        StartCoroutine(ComponentListClose());
    }

    #endregion
}
