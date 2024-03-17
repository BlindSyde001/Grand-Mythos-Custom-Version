using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Serialization;

public class TacticsMenuActions : MenuContainer
{
    [FormerlySerializedAs("segmentsWarning")] public GameObject SegmentsWarning;

    public UIElementList<Button> HeroSelections = new();
    public List<Button> PageList;
    [FormerlySerializedAs("tacticsModules")] public List<TacticsModuleContainer> TacticsModules;
    [FormerlySerializedAs("newComponentList")] public List<NewComponentContainer> NewComponentList;

    Tactics _tacticCndToChange;
    ActionCondition _cndToBecome;
    Tactics _tacticToChange;
    int _tacticActionOrderToChange = 4;
    IAction _actionToBecome;
    int _tacticActionListOrder;
    TacticsModuleContainer _currentContainer;

    readonly List<IAction> _actionsList = new();
    HeroExtension _selectedHero;

    //METHODS
    public override IEnumerable Open(MenuInputs menuInputs)
    {
        gameObject.SetActive(true);
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-800, 480, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(500, 470, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(230, -100, 0), menuInputs.Speed);
        SetHeroSelection();
        SetTacticsList(GameManager.PartyLineup[0]);
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
        gameObject.transform.GetChild(3).DOLocalMove(new Vector3(-740, 328, 0), MenuInputs.Speed);
        gameObject.transform.GetChild(4).DOLocalMove(new Vector3(-710, -100, 0), MenuInputs.Speed);
        yield return new WaitForSeconds(MenuInputs.Speed);
    }

    IEnumerator ComponentListClose()
    {
        gameObject.transform.GetChild(3).DOLocalMove(new Vector3(-1300, 328, 0), MenuInputs.Speed);
        gameObject.transform.GetChild(4).DOLocalMove(new Vector3(-1300, -100, 0), MenuInputs.Speed);
        _tacticToChange = null;
        _tacticCndToChange = null;
        _currentContainer = null;
        yield return new WaitForSeconds(MenuInputs.Speed);
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
        _selectedHero = hero;
        StartCoroutine(ComponentListClose());
        ResetActionSegments();
        // Configure all the TacticsList Buttons: On/Off, Select Cnd, Select Action
        for (int i = 0; i < hero.Tactics.Length; i++)
        {
            int j = i;
            TacticsModules[i].onToggle.text = hero.Tactics[i].IsOn ? "On" : "Off";
            if (hero.Tactics[i].Condition != null)
            {
                TacticsModules[i].condition.text = hero.Tactics[i].Condition.name;
            }
            else
            {
                TacticsModules[i].condition.text = "";
            }

            // Go through all my Actions and turn on their respective Buttons (Based on Segment Cost)
            for (int k = 0; k < hero.Tactics[i].Actions.Length; k++)
            {
                if (hero.Tactics[i].Actions[k] != null)
                {
                    SetActionsBasedOnSegmentCost(hero.Tactics[i].Actions[k].ATBCost, i, k);
                }
                else
                {
                    SetActionsBasedOnSegmentCost(0, i, k);
                }
            }

            TacticsModules[i].onToggleBtn.onClick.RemoveAllListeners();
            TacticsModules[i].conditionBtn.onClick.RemoveAllListeners();

            TacticsModules[i].onToggleBtn.onClick.AddListener(delegate { ToggleTactics(TacticsModules[j], hero.Tactics[j]); });
            TacticsModules[i].conditionBtn.onClick.AddListener(delegate { SelectTacticCnd(TacticsModules[j], hero.Tactics[j]); });
        }
    }
    #region Action Segment Methods

    void ResetActionSegments()
    {
        foreach(TacticsModuleContainer t in TacticsModules)
        {
            t.actionAllowance = 0;
            foreach(Button a in t.singleActionBtns)
            {
                a.gameObject.SetActive(false);
            }
            foreach (Button a in t.doubleActionBtns)
            {
                a.gameObject.SetActive(false);
            }
            foreach (Button a in t.tripleActionBtns)
            {
                a.gameObject.SetActive(false);
            }
            t.quadActionBtn.gameObject.SetActive(false);

            for(int i = 0; i < t.addActionBtns.Count; i++)
            {
                if(i == 0)
                {
                    t.addActionBtns[i].gameObject.SetActive(true);
                }
                else
                {
                    t.addActionBtns[i].gameObject.SetActive(false);
                }
            }
        }
    }

    void SetActionsBasedOnSegmentCost(uint cost, int tOrder, int aOrder)
    {
        int allowance = (int)TacticsModules[tOrder].actionAllowance;
        if (allowance + cost > 4)
        {
            Debug.Log("Not enough Segments!");
            ReOrderActions(tOrder, aOrder);
            return;
        }
        switch (cost)
        {           // Turn on Button >> Set the Name >> Allocate the Segment Allowance
            case 1:
                TacticsModules[tOrder].singleActionBtns[allowance].gameObject.SetActive(true);
                TacticsModules[tOrder].singlesText[allowance].text = _selectedHero.Tactics[tOrder].Actions[aOrder].Name;
                TacticsModules[tOrder].actionAllowance += _selectedHero.Tactics[tOrder].Actions[aOrder].ATBCost;

                TacticsModules[tOrder].singleActionBtns[allowance].onClick.RemoveAllListeners();
                TacticsModules[tOrder].singleActionBtns[allowance].onClick.AddListener(delegate { SelectTacticAction(TacticsModules[tOrder], _selectedHero.Tactics[tOrder], aOrder); });
                break;

            case 2:
                TacticsModules[tOrder].doubleActionBtns[allowance].gameObject.SetActive(true);
                TacticsModules[tOrder].doublesText[allowance].text = _selectedHero.Tactics[tOrder].Actions[aOrder].Name;
                TacticsModules[tOrder].actionAllowance += _selectedHero.Tactics[tOrder].Actions[aOrder].ATBCost;

                TacticsModules[tOrder].doubleActionBtns[allowance].onClick.RemoveAllListeners();
                TacticsModules[tOrder].doubleActionBtns[allowance].onClick.AddListener(delegate { SelectTacticAction(TacticsModules[tOrder], _selectedHero.Tactics[tOrder], aOrder); });
                break;

            case 3:
                TacticsModules[tOrder].tripleActionBtns[allowance].gameObject.SetActive(true);
                TacticsModules[tOrder].triplesText[allowance].text = _selectedHero.Tactics[tOrder].Actions[aOrder].Name;
                TacticsModules[tOrder].actionAllowance += _selectedHero.Tactics[tOrder].Actions[aOrder].ATBCost;

                TacticsModules[tOrder].tripleActionBtns[allowance].onClick.RemoveAllListeners();
                TacticsModules[tOrder].tripleActionBtns[allowance].onClick.AddListener(delegate { SelectTacticAction(TacticsModules[tOrder], _selectedHero.Tactics[tOrder], aOrder); });
                break;

            case 4:
                TacticsModules[tOrder].quadActionBtn.gameObject.SetActive(true);
                TacticsModules[tOrder].quadruplesText.text = _selectedHero.Tactics[tOrder].Actions[aOrder].Name;
                TacticsModules[tOrder].actionAllowance += _selectedHero.Tactics[tOrder].Actions[aOrder].ATBCost;

                TacticsModules[tOrder].quadActionBtn.onClick.RemoveAllListeners();
                TacticsModules[tOrder].quadActionBtn.onClick.AddListener(delegate { SelectTacticAction(TacticsModules[tOrder], _selectedHero.Tactics[tOrder], aOrder); });
                break;
        }
        foreach(Button btn in TacticsModules[tOrder].addActionBtns)
        {
            btn.gameObject.SetActive(false);
        }
        if (TacticsModules[tOrder].actionAllowance == 4)
        {
            return;
        }
        else if(TacticsModules[tOrder].actionAllowance < 4)
        {
            var btn = TacticsModules[tOrder].addActionBtns[(int)TacticsModules[tOrder].actionAllowance];
            btn.gameObject.SetActive(true);
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(delegate { SelectTacticAction(TacticsModules[tOrder], _selectedHero.Tactics[tOrder], FindEmptyActionSlot(tOrder)); });
        }
    }

    IEnumerator SendWarning()
    {
        SegmentsWarning.SetActive(true);
        yield return new WaitForSeconds(2f);
        SegmentsWarning.SetActive(false);
    }

    void ReOrderActions(int tOrder, int aOrder)
    {
        uint eAllowance = 0;
        for (int i = 0; i < _selectedHero.Tactics[tOrder].Actions.Length; i++)
        {
            if (_selectedHero.Tactics[tOrder].Actions[i] != null)
            {
                if (eAllowance + _selectedHero.Tactics[tOrder].Actions[i].ATBCost > 4)
                {
                    StartCoroutine(SendWarning());
                    _selectedHero.Tactics[tOrder].Actions[i] = null;
                }
                else
                {
                    eAllowance += _selectedHero.Tactics[tOrder].Actions[i].ATBCost;
                }
            }
        }
    }

    int FindEmptyActionSlot(int tOrder)
    {
        for(int i = 0; i < _selectedHero.Tactics[tOrder].Actions.Length; i++)
        {
            if(_selectedHero.Tactics[tOrder].Actions[i] == null)
            {
                return i;
            }
        }
        return 0;
    }
    #endregion
    #region Open Page of Conditions/Actions
    public void SetPages(bool isCnd)
    {
        for(int i = 0; i < PageList.Count; i++)
        {
            int j = i;
            PageList[i].onClick.RemoveAllListeners();
            PageList[i].onClick.AddListener(isCnd? delegate { SetCndList(j); } : delegate { SetActionList(j); });
        }
    }
    public void SetCndList(int pageNo)
    {   // Change Cnds represented on the CndList to match the Page Number
        for(int i = 0; i < NewComponentList.Count; i++) // iterate through the Buttons
        {
            NewComponentList[i].cmpName.text = "";
            if(InventoryManager.ConditionsAcquired.Count > (pageNo * 10) + i) // Check if there's a cnd in that Slot
            {
                if(InventoryManager.ConditionsAcquired.Contains(InventoryManager.ConditionsAcquired[(pageNo * 10) + i])) // Check if you unlocked that Cnd
                { 
                    NewComponentList[i].selectedCnd = InventoryManager.ConditionsAcquired[(pageNo * 10) + i];
                    NewComponentList[i].cmpName.text = NewComponentList[i].selectedCnd.name;

                    int j = i;
                    NewComponentList[i].cmpButton.onClick.RemoveAllListeners();
                    NewComponentList[i].cmpButton.onClick.AddListener(delegate {SelectNewListCnd(NewComponentList[j].selectedCnd); });
                }
            }
        }
    }
    public void SetActionList(int pageNo)
    {
        _actionsList.Clear();
        _actionsList.Add(_selectedHero.BasicAttack);
        foreach(var (item, _) in InventoryManager.Enumerate<Consumable>())
        {
            _actionsList.Add(item);
        }
        foreach(var a in _selectedHero.Skills)
        {
            _actionsList.Add(a);
        }

        for (int i = 0; i < NewComponentList.Count; i++) // iterate through the Buttons
        {
            NewComponentList[i].cmpName.text = "";
            if (_actionsList.Count > (pageNo * 10) + i) // Set new Page up
            {
                if (_actionsList[(pageNo * 10) + i] is Skill skill
                    && (_selectedHero.Skills.Contains(skill) || _selectedHero.BasicAttack == skill))
                {
                    NewComponentList[i].selectedAction = _actionsList[(pageNo * 10) + i];
                    NewComponentList[i].cmpName.text = NewComponentList[i].selectedAction.Name;

                    int j = i;
                    NewComponentList[i].cmpButton.onClick.RemoveAllListeners();
                    NewComponentList[i].cmpButton.onClick.AddListener(delegate { SelectNewListAction(NewComponentList[j].selectedAction); });
                }
                else
                {
                    foreach(var (item, _) in InventoryManager.Enumerate<Consumable>())
                    {
                        if (item.name == _actionsList[(pageNo * 10) + i].Name)
                        {
                            NewComponentList[i].selectedAction = _actionsList[(pageNo * 10) + i];
                            NewComponentList[i].cmpName.text = NewComponentList[i].selectedAction.Name;

                            int j = i;
                            NewComponentList[i].cmpButton.onClick.RemoveAllListeners();
                            NewComponentList[i].cmpButton.onClick.AddListener(delegate { SelectNewListAction(NewComponentList[j].selectedAction); });
                        }
                    }
                }
            }
        }
    }
    #endregion

    public void ToggleTactics(TacticsModuleContainer thisContainer, Tactics tactic)
    {
        if(tactic.Condition == null || !CheckActionsStatus(tactic))
        {
            tactic.IsOn = false;
            thisContainer.onToggle.text = "Off";
            return;
        }
        tactic.IsOn = !tactic.IsOn;
        thisContainer.onToggle.text = tactic.IsOn ? "On" : "Off";
    }

    bool CheckActionsStatus(Tactics tactic)
    {
        foreach(IAction action in tactic.Actions)
        {
            if(action != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
    #region Swapping Conditions
    public void SelectTacticCnd(TacticsModuleContainer thisContainer, Tactics tactic)
    {
        // CLICK BUTTON OF CND U WANT TO CHANGE
        // Designate this Cnd Space to be swapped with a new Cnd, or if double clicked, reset
        if (_tacticCndToChange != tactic)
        {
            _tacticCndToChange = tactic;
            _currentContainer = thisContainer;
            _tacticToChange = null;
            StartCoroutine(ComponentListOpen());
            SetPages(true);
            SetCndList(0);
        }
        else
        {
            _currentContainer = null;
            _tacticCndToChange = null;
            StartCoroutine(ComponentListClose());
        }
    }
    public void SelectNewListCnd(ActionCondition cnd)
    {
        _cndToBecome = cnd;
        SwapCnds();
        _currentContainer = null;
        _tacticCndToChange = null;
        StartCoroutine(ComponentListClose());
    }

    void SwapCnds()
    {
        _tacticCndToChange.Condition = _cndToBecome;
        _currentContainer.condition.text = _tacticCndToChange.Condition.name;
    }
    #endregion
    #region Swapping Actions
    public void SelectTacticAction(TacticsModuleContainer thisContainer, Tactics tactic, int aOrder)
    {
        // CLICK BUTTON OF ACTION U WANT TO CHANGE
        // Designate this Action Space to be swapped with a new Action, or if double clicked, reset
        if (_tacticToChange != tactic)
        {
            _tacticToChange = tactic;
            _tacticActionOrderToChange = aOrder;
            _currentContainer = thisContainer;
            _tacticCndToChange = null;
            StartCoroutine(ComponentListOpen());
            SetPages(false);
            SetActionList(0);
        }
        else
        {
            _tacticToChange = null;
            _tacticActionOrderToChange = 4;
            _currentContainer = null;
            StartCoroutine(ComponentListClose());
        }
    }
    public void SelectNewListAction(IAction action)
    {
        _actionToBecome = action;
        SwapActions();
        _currentContainer = null;
        _tacticToChange = null;
        StartCoroutine(ComponentListClose());
    }

    void SwapActions()
    {
        // Swap Action Function
        _tacticToChange.Actions[_tacticActionOrderToChange] = _actionToBecome;

        // Swap Action UI
        SetTacticsList(_selectedHero);
    }
    #endregion
}
