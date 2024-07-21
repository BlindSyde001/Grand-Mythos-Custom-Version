using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.InputSystem;

public class TacticsMenuActions : MenuContainerWithHeroSelection
{
    public GameObject SegmentsWarning;

    public List<Button> PageList;
    public List<TacticsModuleContainer> TacticsModules;
    public List<NewComponentContainer> NewComponentList;
    [Required] public RectTransform NewComponentParentRect;
    [Required] public InputActionReference SwitchPage;
    [Required] public ActionSetContainer ActionSet1, ActionSet2;
    int _currentPage;

    Button _dropdownSource;

    readonly List<IAction> _actionsList = new();

    //METHODS
    public override IEnumerable Open(MenuInputs menuInputs)
    {
        foreach (var yields in base.Open(menuInputs))
        {
            yield return yields;
        }

        gameObject.SetActive(true);
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(500, 470, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(230, -100, 0), menuInputs.Speed);
        QuickFade(ActionSet1.gameObject, 1, menuInputs.Speed);
        QuickFade(ActionSet2.gameObject, 1, menuInputs.Speed);

        yield return new WaitForSeconds(menuInputs.Speed);
    }

    public override IEnumerable Close(MenuInputs menuInputs)
    {
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(500, 610, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(1700, -100, 0), menuInputs.Speed);
        NewComponentParentRect.DOLocalMove(new Vector3(-1300, -100, 0), menuInputs.Speed);
        SwitchPage.action.performed -= SwitchPagePerformed;
        QuickFade(ActionSet1.gameObject, 0, menuInputs.Speed);
        QuickFade(ActionSet2.gameObject, 0, menuInputs.Speed);

        yield return new WaitForSeconds(menuInputs.Speed);

        NewComponentParentRect.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    void QuickFade(GameObject target, float goalAlpha, float speed)
    {
        foreach (var image in target.GetComponentsInChildren<Graphic>())
        {
            if (image.isActiveAndEnabled == false || image.gameObject.activeInHierarchy == false)
                continue;
            var col = image.color;
            col.a = 1-goalAlpha;
            image.color = col;
            image.DOFade(goalAlpha, speed);
        }
    }

    protected override void OnSelectedHeroChanged()
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine(ComponentListClose());
        // Configure all the TacticsList Buttons: On/Off, Select Cnd, Select Action
        for (int i = 0; i < TacticsModules.Count; i++)
            SetupField(i);

        SetupActionSet(ActionSet1, SelectedHero.Actionset1);
        SetupActionSet(ActionSet2, SelectedHero.Actionset2);
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

    void PrepareAndEnumerateDisplayFor(IEnumerable<IAction> actions, ActionSetContainer actionSetContainer, Action<Button, int> OnPressAdd)
    {
        var tacticsUI = actionSetContainer;
        foreach(var button in tacticsUI.singleActionBtns)
            button.gameObject.SetActive(false);
        foreach (var button in tacticsUI.doubleActionBtns)
            button.gameObject.SetActive(false);
        foreach (var button in tacticsUI.tripleActionBtns)
            button.gameObject.SetActive(false);
        tacticsUI.quadActionBtn.gameObject.SetActive(false);

        for(int j = 0; j < tacticsUI.addActionBtns.Count; j++)
            tacticsUI.addActionBtns[j].gameObject.SetActive(false);

        uint costTotal = 0;
        int actionIndex = 0;
        foreach (var action in actions)
        {
            Button actionSelection;
            TextMeshProUGUI textProvider;
            switch (action.ActionCost)
            {
                case 1:
                    actionSelection = tacticsUI.singleActionBtns[(int)costTotal];
                    textProvider = tacticsUI.singlesText[(int)costTotal];
                    break;

                case 2:
                    actionSelection = tacticsUI.doubleActionBtns[(int)costTotal];
                    textProvider = tacticsUI.doublesText[(int)costTotal];
                    break;

                case 3:
                    actionSelection = tacticsUI.tripleActionBtns[(int)costTotal];
                    textProvider = tacticsUI.triplesText[(int)costTotal];
                    break;

                case 4:
                    actionSelection = tacticsUI.quadActionBtn;
                    textProvider = tacticsUI.quadruplesText;
                    break;

                default:
                    throw new NotImplementedException();
            }

            var currentIndex = actionIndex;
            actionIndex++;

            textProvider.text = action.Name;
            costTotal += action.ActionCost;
            actionSelection.gameObject.SetActive(true);
            actionSelection.onClick.RemoveAllListeners();
            actionSelection.onClick.AddListener(() => OnPressAdd(actionSelection, currentIndex));
        }

        if (costTotal > SelectedHero.ActionChargeMax)
        {
            StartCoroutine(SendLackOfSegmentsWarning());
        }
        else if (costTotal < SelectedHero.ActionChargeMax)
        {
            var btn = tacticsUI.addActionBtns[(int)costTotal];
            btn.gameObject.SetActive(true);
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnPressAdd(btn, actionIndex));
        }
    }

    void SetupActionSet(ActionSetContainer ui, IActionCollection set)
    {
        PrepareAndEnumerateDisplayFor(set, ui, OnClickAction);

        void OnClickAction(Button pressedButton, int index)
        {
            ShowActionDropdown(SelectedHero, action => OnSetAction(action, index), pressedButton);
        }

        void OnSetAction(IAction newAction, int actionIndex)
        {
            if (newAction is null)
            {
                var newActions = new List<IAction>(set.BackingArray);
                newActions.RemoveAt(actionIndex);
                set.BackingArray = newActions.ToArray();
            }
            else
            {
                var newActions = new List<IAction>(set.BackingArray);
                if (actionIndex >= newActions.Count)
                    newActions.Add(newAction);
                else
                    newActions[actionIndex] = newAction;

                while (newActions.CostTotal() > SelectedHero.ActionChargeMax && newActions.Count > 1)
                {
                    newActions.RemoveAt(newActions.Count-1);
                    if (newActions.Count - 1 <= actionIndex)
                        newActions[^1] = newAction;
                }

                if (newActions.CostTotal() > SelectedHero.ActionChargeMax)
                    StartCoroutine(SendLackOfSegmentsWarning());
                else
                    set.BackingArray = newActions.ToArray();
            }

            SetupActionSet(ui, set);
        }
    }

    void SetupField(int i)
    {
        var heroTactic = i < SelectedHero.Tactics.Length ? SelectedHero.Tactics[i] : null;

        var tacticsUI = TacticsModules[i];
        var actions = heroTactic?.Actions ?? Enumerable.Empty<IAction>();

        PrepareAndEnumerateDisplayFor(actions, tacticsUI, OnAddAction);

        tacticsUI.onToggle.text = heroTactic?.IsOn == true ? "On" : "Off";
        tacticsUI.condition.text = heroTactic?.Condition != null ? heroTactic.Condition.name : "";
        tacticsUI.onToggleBtn.interactable = heroTactic?.Condition != null && heroTactic.Actions.Length > 0;
        tacticsUI.onToggleBtn.onClick.RemoveAllListeners();
        tacticsUI.onToggleBtn.onClick.AddListener(OnToggleTacticPressed);

        tacticsUI.conditionBtn.onClick.RemoveAllListeners();
        tacticsUI.conditionBtn.onClick.AddListener(() => ShowConditionDropdown(OnSetCondition, tacticsUI.conditionBtn));

        void OnAddAction(Button pressedButton, int index)
        {
            ShowActionDropdown(SelectedHero, action => OnSetAction(action, index), pressedButton);
        }

        void OnSetAction(IAction newAction, int actionIndex)
        {
            if (i >= SelectedHero.Tactics.Length)
            {
                var newTactics = new Tactics[i + 1];
                SelectedHero.Tactics.CopyTo(newTactics, 0);
                SelectedHero.Tactics = newTactics;
            }
            SelectedHero.Tactics[i] ??= new() { IsOn = false };

            var tactic = SelectedHero.Tactics[i];
            var newActions = new List<IAction>(tactic.Actions.BackingArray);
            if (newAction is null)
            {
                newActions.RemoveAt(actionIndex);
                tactic.Actions = new(){ BackingArray = newActions.ToArray() };
            }
            else
            {
                if (actionIndex >= newActions.Count)
                    newActions.Add(newAction);
                else
                    newActions[actionIndex] = newAction;

                while (newActions.CostTotal() > SelectedHero.ActionChargeMax && newActions.Count > 1)
                {
                    newActions.RemoveAt(newActions.Count-1);
                    if (newActions.Count - 1 <= actionIndex)
                        newActions[^1] = newAction;
                }

                if (newActions.CostTotal() > SelectedHero.ActionChargeMax)
                    StartCoroutine(SendLackOfSegmentsWarning());
                else
                    tactic.Actions = new(){ BackingArray = newActions.ToArray() };
            }

            SetupField(i);
        }

        void OnToggleTacticPressed()
        {
            if (i >= SelectedHero.Tactics.Length)
                return;

            var tactic = SelectedHero.Tactics[i];
            if (tactic is not null && tactic.Condition != null && tactic.Actions.Length > 0)
                tactic.IsOn = !tactic.IsOn;
            SetupField(i);
        }

        void OnSetCondition(ActionCondition newCondition)
        {
            if (i >= SelectedHero.Tactics.Length)
            {
                var newTactics = new Tactics[i + 1];
                SelectedHero.Tactics.CopyTo(newTactics, 0);
                SelectedHero.Tactics = newTactics;
            }

            var tactic = SelectedHero.Tactics[i] ??= new() { IsOn = false };
            tactic.Condition = newCondition;
            SetupField(i);
        }
    }

    #region Action Segment Methods
    IEnumerator SendLackOfSegmentsWarning()
    {
        SegmentsWarning.SetActive(true);
        yield return new WaitForSeconds(2f);
        SegmentsWarning.SetActive(false);
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

    public void PopulateActionList(HeroExtension selectedHero, int pageIndex, Action<IAction> OnNewAction)
    {
        _actionsList.Clear();
        _actionsList.Add(null);
        _actionsList.Add(selectedHero.BasicAttack);
        foreach(var a in selectedHero.Skills)
            _actionsList.Add(a);
        foreach(var (item, _) in InventoryManager.Enumerate<Consumable>())
            _actionsList.Add(item);

        foreach (var component in NewComponentList)
            component.cmpButton.onClick.RemoveAllListeners();

        bool setSelection = false;
        for (int i = 0, j = pageIndex * NewComponentList.Count; i < NewComponentList.Count; i++, j++) // iterate through the Buttons
        {
            var ui = NewComponentList[i];
            ui.cmpButton.onClick.RemoveAllListeners();

            if (j >= _actionsList.Count)
            {
                ui.cmpButton.interactable = false;
                ui.cmpName.text = "";
                continue; // Continue the loop resetting buttons appropriately
            }

            if (_actionsList[j] is Skill skill)
            {
                ui.cmpButton.interactable = selectedHero.Skills.Contains(skill) || selectedHero.BasicAttack == skill;
            }
            else
            {
                ui.cmpButton.interactable = true;
            }

            ui.selectedAction = _actionsList[j];
            ui.cmpName.text = ui.selectedAction?.Name ?? "None";
            ui.cmpButton.onClick.AddListener(() => SelectNewListAction(selectedHero, ui.selectedAction, OnNewAction));
            if (setSelection == false && ui.cmpButton.interactable)
            {
                setSelection = true;
                ui.cmpButton.Select();
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
