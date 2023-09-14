using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TacticsMenuActions : MonoBehaviour
{
    // VARIABLES
    private MenuInputs menuInputs;
    private InputManager inputManager;
    private GameManager gameManager;
    private InventoryManager inventoryManager;

    private HeroExtension selectedHero;
    private bool listCoroutineRunning;

    private List<IAction> ActionsList = new();
    public GameObject segmentsWarning;

    #region GameObject References
    public List<Button> heroSelections;
    public List<Button> PageList;
    public List<TacticsModuleContainer> tacticsModules;
    public List<NewComponentContainer> newComponentList;
    #endregion
    #region Customising Tactics
    private Tactics tacticCndToChange;
    private ActionCondition cndToBecome;

    private Tactics tacticToChange;
    private int tacticActionOrderToChange = 4;
    private IAction actionToBecome;
    private int tacticActionListOrder;
    private TacticsModuleContainer currentContainer;
    #endregion

    // UPDATES
    private void Start()
    {
        gameManager = GameManager._instance;
        menuInputs = FindObjectOfType<MenuInputs>();
        inputManager = InputManager._instance;
        inventoryManager = InventoryManager._instance;
    }

    //METHODS
    public IEnumerator TacticsMenuOpen()
    {
        if (!menuInputs.coroutineRunning)
        {
            yield return new WaitForSeconds(menuInputs.speed);
            inputManager.MenuItems[5].SetActive(true);
            inputManager.MenuItems[5].transform.GetChild(0).DOLocalMove(new Vector3(-800, 480, 0), menuInputs.speed);
            inputManager.MenuItems[5].transform.GetChild(1).DOLocalMove(new Vector3(500, 470, 0), menuInputs.speed);
            inputManager.MenuItems[5].transform.GetChild(2).DOLocalMove(new Vector3(230, -100, 0), menuInputs.speed);
            SetHeroSelection();
            SetTacticsList(gameManager._PartyLineup[0]);
        }
    }
    public IEnumerator TacticsMenuClose(bool closeAllOverride)
    {
        if (!menuInputs.coroutineRunning)
        {
            menuInputs.coroutineRunning = true;
            inputManager.MenuItems[5].transform.GetChild(0).DOLocalMove(new Vector3(-1350, 480, 0), menuInputs.speed);
            inputManager.MenuItems[5].transform.GetChild(1).DOLocalMove(new Vector3(500, 610, 0), menuInputs.speed);
            inputManager.MenuItems[5].transform.GetChild(2).DOLocalMove(new Vector3(1700, -100, 0), menuInputs.speed);
            inputManager.MenuItems[5].transform.GetChild(3).DOLocalMove(new Vector3(-1300, 328, 0), menuInputs.speed);
            inputManager.MenuItems[5].transform.GetChild(4).DOLocalMove(new Vector3(-1300, -100, 0), menuInputs.speed);
            yield return new WaitForSeconds(menuInputs.speed);
            inputManager.MenuItems[5].SetActive(false);
            menuInputs.coroutineRunning = false;

        }
        if (!closeAllOverride)
        {
            menuInputs.startMenuActions.StartMenuOpen();
            yield return new WaitForSeconds(menuInputs.speed);
            menuInputs.currentMenuOpen = 0;
        }
    }

    private IEnumerator ComponentListOpen()
    {
        if (!listCoroutineRunning)
        {
            listCoroutineRunning = true;
            inputManager.MenuItems[5].transform.GetChild(3).DOLocalMove(new Vector3(-740, 328, 0), menuInputs.speed);
            inputManager.MenuItems[5].transform.GetChild(4).DOLocalMove(new Vector3(-710, -100, 0), menuInputs.speed);
            yield return new WaitForSeconds(menuInputs.speed);
            listCoroutineRunning = false;
        }
    }
    private IEnumerator ComponentListClose()
    {
        if(!listCoroutineRunning)
        {
            listCoroutineRunning = true;
            inputManager.MenuItems[5].transform.GetChild(3).DOLocalMove(new Vector3(-1300, 328, 0), menuInputs.speed);
            inputManager.MenuItems[5].transform.GetChild(4).DOLocalMove(new Vector3(-1300, -100, 0), menuInputs.speed);
            tacticToChange = null;
            tacticCndToChange = null;
            currentContainer = null;
            yield return new WaitForSeconds(menuInputs.speed);
            listCoroutineRunning = false;
        }
    }



    internal void SetHeroSelection()
    {
        foreach (Button a in heroSelections)
        {
            a.gameObject.SetActive(false);
            a.onClick.RemoveAllListeners();
        }
        for (int i = 0; i < gameManager._PartyLineup.Count; i++)
        {
            int j = i;
            heroSelections[i].gameObject.SetActive(true);
            heroSelections[i].GetComponent<Image>().sprite = gameManager._PartyLineup[j].charPortrait;
            heroSelections[i].onClick.AddListener(delegate { SetTacticsList(gameManager._PartyLineup[j]); });
        }
    }
    public void SetTacticsList(HeroExtension hero)
    {
        selectedHero = hero;
        StartCoroutine(ComponentListClose());
        ResetActionSegments();
        // Configure all the TacticsList Buttons: On/Off, Select Cnd, Select Action
        for (int i = 0; i < hero.Tactics.Length; i++)
        {
            int j = i;
            tacticsModules[i].onToggle.text = hero.Tactics[i].IsOn ? "On" : "Off";
            if (hero.Tactics[i].Condition != null)
            { 
                tacticsModules[i].condition.text = hero.Tactics[i].Condition.name;
            } 
            else
            {
                tacticsModules[i].condition.text = "";
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

            tacticsModules[i].onToggleBtn.onClick.RemoveAllListeners();
            tacticsModules[i].conditionBtn.onClick.RemoveAllListeners();

            tacticsModules[i].onToggleBtn.onClick.AddListener(delegate { ToggleTactics(tacticsModules[j], hero.Tactics[j]); });
            tacticsModules[i].conditionBtn.onClick.AddListener(delegate { SelectTacticCnd(tacticsModules[j], hero.Tactics[j]); });
        }
    }
    #region Action Segment Methods
    private void ResetActionSegments()
    {
        foreach(TacticsModuleContainer t in tacticsModules)
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
    private void SetActionsBasedOnSegmentCost(uint cost, int tOrder, int aOrder)
    {
        int allowance = (int)tacticsModules[tOrder].actionAllowance;
        if (allowance + cost > 4)
        {
            Debug.Log("Not enough Segments!");
            ReOrderActions(tOrder, aOrder);
            return;
        }
        switch (cost)
        {           // Turn on Button >> Set the Name >> Allocate the Segment Allowance
            case 1:
                tacticsModules[tOrder].singleActionBtns[allowance].gameObject.SetActive(true);
                tacticsModules[tOrder].singlesText[allowance].text = selectedHero.Tactics[tOrder].Actions[aOrder].Name;
                tacticsModules[tOrder].actionAllowance += selectedHero.Tactics[tOrder].Actions[aOrder].ATBCost;

                tacticsModules[tOrder].singleActionBtns[allowance].onClick.RemoveAllListeners();
                tacticsModules[tOrder].singleActionBtns[allowance].onClick.AddListener(delegate { SelectTacticAction(tacticsModules[tOrder], selectedHero.Tactics[tOrder], aOrder); });
                break;

            case 2:
                tacticsModules[tOrder].doubleActionBtns[allowance].gameObject.SetActive(true);
                tacticsModules[tOrder].doublesText[allowance].text = selectedHero.Tactics[tOrder].Actions[aOrder].Name;
                tacticsModules[tOrder].actionAllowance += selectedHero.Tactics[tOrder].Actions[aOrder].ATBCost;

                tacticsModules[tOrder].doubleActionBtns[allowance].onClick.RemoveAllListeners();
                tacticsModules[tOrder].doubleActionBtns[allowance].onClick.AddListener(delegate { SelectTacticAction(tacticsModules[tOrder], selectedHero.Tactics[tOrder], aOrder); });
                break;

            case 3:
                tacticsModules[tOrder].tripleActionBtns[allowance].gameObject.SetActive(true);
                tacticsModules[tOrder].triplesText[allowance].text = selectedHero.Tactics[tOrder].Actions[aOrder].Name;
                tacticsModules[tOrder].actionAllowance += selectedHero.Tactics[tOrder].Actions[aOrder].ATBCost;

                tacticsModules[tOrder].tripleActionBtns[allowance].onClick.RemoveAllListeners();
                tacticsModules[tOrder].tripleActionBtns[allowance].onClick.AddListener(delegate { SelectTacticAction(tacticsModules[tOrder], selectedHero.Tactics[tOrder], aOrder); });
                break;

            case 4:
                tacticsModules[tOrder].quadActionBtn.gameObject.SetActive(true);
                tacticsModules[tOrder].quadruplesText.text = selectedHero.Tactics[tOrder].Actions[aOrder].Name;
                tacticsModules[tOrder].actionAllowance += selectedHero.Tactics[tOrder].Actions[aOrder].ATBCost;

                tacticsModules[tOrder].quadActionBtn.onClick.RemoveAllListeners();
                tacticsModules[tOrder].quadActionBtn.onClick.AddListener(delegate { SelectTacticAction(tacticsModules[tOrder], selectedHero.Tactics[tOrder], aOrder); });
                break;
        }
        foreach(Button btn in tacticsModules[tOrder].addActionBtns)
        {
            btn.gameObject.SetActive(false);
        }
        if (tacticsModules[tOrder].actionAllowance == 4)
        {
            return;
        }
        else if(tacticsModules[tOrder].actionAllowance < 4)
        {
            var btn = tacticsModules[tOrder].addActionBtns[(int)tacticsModules[tOrder].actionAllowance];
            btn.gameObject.SetActive(true);
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(delegate { SelectTacticAction(tacticsModules[tOrder], selectedHero.Tactics[tOrder], FindEmptyActionSlot(tOrder)); });
        }
    }
    private IEnumerator SendWarning()
    {
        segmentsWarning.SetActive(true);
        yield return new WaitForSeconds(2f);
        segmentsWarning.SetActive(false);
    }
    private void ReOrderActions(int tOrder, int aOrder)
    {
        uint eAllowance = 0;
        for (int i = 0; i < selectedHero.Tactics[tOrder].Actions.Length; i++)
        {
            if (selectedHero.Tactics[tOrder].Actions[i] != null)
            {
                if (eAllowance + selectedHero.Tactics[tOrder].Actions[i].ATBCost > 4)
                {
                    StartCoroutine(SendWarning());
                    selectedHero.Tactics[tOrder].Actions[i] = null;
                }
                else
                {
                    eAllowance += selectedHero.Tactics[tOrder].Actions[i].ATBCost;
                }
            }
        }
    }
    private int FindEmptyActionSlot(int tOrder)
    {
        for(int i = 0; i < selectedHero.Tactics[tOrder].Actions.Length; i++)
        {
            if(selectedHero.Tactics[tOrder].Actions[i] == null)
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
        for(int i = 0; i < newComponentList.Count; i++) // iterate through the Buttons
        {
            newComponentList[i].cmpName.text = "";
            if(inventoryManager.ConditionsAcquired.Count > (pageNo * 10) + i) // Check if there's a cnd in that Slot
            {
                if(inventoryManager.ConditionsAcquired.Contains(inventoryManager.ConditionsAcquired[(pageNo * 10) + i])) // Check if you unlocked that Cnd
                { 
                    newComponentList[i].selectedCnd = inventoryManager.ConditionsAcquired[(pageNo * 10) + i];
                    newComponentList[i].cmpName.text = newComponentList[i].selectedCnd.name;

                    int j = i;
                    newComponentList[i].cmpButton.onClick.RemoveAllListeners();
                    newComponentList[i].cmpButton.onClick.AddListener(delegate {SelectNewListCnd(newComponentList[j].selectedCnd); });
                }
            }
        }
    }
    public void SetActionList(int pageNo)
    {
        ActionsList.Clear();
        ActionsList.Add(selectedHero.BasicAttack);
        foreach(var a in inventoryManager.ConsumablesInBag)
        {
            ActionsList.Add((Consumable)a.thisItem);
        }
        foreach(var a in selectedHero.Skills)
        {
            ActionsList.Add(a);
        }

        for (int i = 0; i < newComponentList.Count; i++) // iterate through the Buttons
        {
            newComponentList[i].cmpName.text = "";
            if (ActionsList.Count > (pageNo * 10) + i) // Set new Page up
            {
                if (ActionsList[(pageNo * 10) + i] is Skill skill
                    && (selectedHero.Skills.Contains(skill) || selectedHero.BasicAttack == skill))
                {
                    newComponentList[i].selectedAction = ActionsList[(pageNo * 10) + i];
                    newComponentList[i].cmpName.text = newComponentList[i].selectedAction.Name;

                    int j = i;
                    newComponentList[i].cmpButton.onClick.RemoveAllListeners();
                    newComponentList[i].cmpButton.onClick.AddListener(delegate { SelectNewListAction(newComponentList[j].selectedAction); });
                }
                else
                {
                    for (int k = 0; k < inventoryManager.ConsumablesInBag.Count; k++)
                    {
                        if(inventoryManager.ConsumablesInBag[k].thisItem.name == ActionsList[(pageNo * 10) + i].Name)
                        {
                            newComponentList[i].selectedAction = ActionsList[(pageNo * 10) + i];
                            newComponentList[i].cmpName.text = newComponentList[i].selectedAction.Name;

                            int j = i;
                            newComponentList[i].cmpButton.onClick.RemoveAllListeners();
                            newComponentList[i].cmpButton.onClick.AddListener(delegate { SelectNewListAction(newComponentList[j].selectedAction); });
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
    private bool CheckActionsStatus(Tactics tactic)
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
        if (!listCoroutineRunning)
        {
            if (tacticCndToChange != tactic)
            {
                tacticCndToChange = tactic;
                currentContainer = thisContainer;
                tacticToChange = null;
                StartCoroutine(ComponentListOpen());
                SetPages(true);
                SetCndList(0);
            }
            else
            {
                currentContainer = null;
                tacticCndToChange = null;
                StartCoroutine(ComponentListClose());
            }
        }
    }
    public void SelectNewListCnd(ActionCondition cnd)
    {
        cndToBecome = cnd;
        SwapCnds();
        currentContainer = null;
        tacticCndToChange = null;
        StartCoroutine(ComponentListClose());
    }
    private void SwapCnds()
    {
        tacticCndToChange.Condition = cndToBecome;
        currentContainer.condition.text = tacticCndToChange.Condition.name;
    }
    #endregion
    #region Swapping Actions
    public void SelectTacticAction(TacticsModuleContainer thisContainer, Tactics tactic, int aOrder)
    {
        // CLICK BUTTON OF ACTION U WANT TO CHANGE
        // Designate this Action Space to be swapped with a new Action, or if double clicked, reset
        if (!listCoroutineRunning)
        {
            if (tacticToChange != tactic)
            {
                tacticToChange = tactic;
                tacticActionOrderToChange = aOrder;
                currentContainer = thisContainer;
                tacticCndToChange = null;
                StartCoroutine(ComponentListOpen());
                SetPages(false);
                SetActionList(0);
            }
            else
            {
                tacticToChange = null;
                tacticActionOrderToChange = 4;
                currentContainer = null;
                StartCoroutine(ComponentListClose());
            }
        }
    }
    public void SelectNewListAction(IAction action)
    {
        actionToBecome = action;
        SwapActions();
        currentContainer = null;
        tacticToChange = null;
        StartCoroutine(ComponentListClose());
    }
    private void SwapActions()
    {
        // Swap Action Function
        tacticToChange.Actions[tacticActionOrderToChange] = actionToBecome;

        // Swap Action UI
        SetTacticsList(selectedHero);
    }
    #endregion
}
