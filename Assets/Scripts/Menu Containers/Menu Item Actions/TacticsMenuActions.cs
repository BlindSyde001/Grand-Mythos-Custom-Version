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
    private List<Action> ActionsList = new List<Action>();

    #region GameObject References
    public List<Button> heroSelections;
    public List<Button> PageList;
    public List<TacticsModuleContainer> tacticsModules;
    public List<NewComponentContainer> newComponentList;
    #endregion
    #region Customising Tactics
    private Tactic tacticCndToChange;
    private Condition cndToBecome;
    private Tactic tacticActionToChange;
    private Action actionToBecome;
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
            tacticActionToChange = null;
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
            heroSelections[i].onClick.AddListener(delegate { SetTacticsList(gameManager._PartyLineup[j]); });
        }
    }
    public void SetTacticsList(HeroExtension hero)
    {
        selectedHero = hero;
        StartCoroutine(ComponentListClose());
        // Configure all the TacticsList Buttons: On/Off, Select Cnd, Select Action
        for (int i = 0; i < hero.myTacticController._TacticsList.Count; i++) 
        {
            int j = i;
            tacticsModules[i].onToggle.text = hero.myTacticController._TacticsList[i].isTurnedOn ? "On" : "Off";
            if (hero.myTacticController._TacticsList[i]._Condition != null)
            { 
                tacticsModules[i].condition.text = hero.myTacticController._TacticsList[i]._Condition.name; 
            } 
            else
            {
                tacticsModules[i].condition.text = "";
            }
            if (hero.myTacticController._TacticsList[i]._Action != null)
            {
                tacticsModules[i].action.text = hero.myTacticController._TacticsList[i]._Action._Name;
            }
            else
            {
                tacticsModules[i].action.text = "";
            }

            tacticsModules[i].onToggleBtn.onClick.RemoveAllListeners();
            tacticsModules[i].conditionBtn.onClick.RemoveAllListeners();
            tacticsModules[i].actionBtn.onClick.RemoveAllListeners();

            tacticsModules[i].onToggleBtn.onClick.AddListener(delegate { ToggleTactics(tacticsModules[j], hero.myTacticController._TacticsList[j]); });
            tacticsModules[i].conditionBtn.onClick.AddListener(delegate { SelectTacticsCnd(tacticsModules[j], hero.myTacticController._TacticsList[j]); });
            tacticsModules[i].actionBtn.onClick.AddListener(delegate { SelectTacticsAction(tacticsModules[j], hero.myTacticController._TacticsList[j]); });
        }
    }


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
            if(gameManager._ConditionsDatabase.Count > (pageNo * 10) + i) // Check if there's a cnd in that Slot
            {
                if(inventoryManager.ConditionsAcquired.Contains(gameManager._ConditionsDatabase[(pageNo * 10) + i])) // Check if you unlocked that Cnd
                { 
                    newComponentList[i].selectedCnd = gameManager._ConditionsDatabase[(pageNo * 10) + i];
                    newComponentList[i].cmpName.text = newComponentList[i].selectedCnd.name;

                    int j = i;
                    newComponentList[i].cmpButton.onClick.RemoveAllListeners();
                    newComponentList[i].cmpButton.onClick.AddListener(delegate {SelectListCnd(newComponentList[j].selectedCnd); });
                }
            }
        }
    }
    public void SetActionList(int pageNo)
    {
        ActionsList.Clear();
        ActionsList.Add(selectedHero._BasicAttack);
        foreach(Action a in gameManager._ItemSkillsDatabase)
        {
            ActionsList.Add(a);
        }
        foreach(Action a in selectedHero._AllUsableActions)
        {
            ActionsList.Add(a);
        }

        for (int i = 0; i < newComponentList.Count; i++) // iterate through the Buttons
        {
            newComponentList[i].cmpName.text = "";
            if (ActionsList.Count > (pageNo * 10) + i) // Set new Page up
            {
                if (selectedHero._AvailableActions.Contains(ActionsList[(pageNo * 10) + i]) ||
                    selectedHero._BasicAttack == ActionsList[(pageNo * 10) + i])
                {
                    newComponentList[i].selectedAction = ActionsList[(pageNo * 10) + i];
                    newComponentList[i].cmpName.text = newComponentList[i].selectedAction._Name;

                    int j = i;
                    newComponentList[i].cmpButton.onClick.RemoveAllListeners();
                    newComponentList[i].cmpButton.onClick.AddListener(delegate { SelectListAction(newComponentList[j].selectedAction); });
                }
                else
                {
                    for (int k = 0; k < inventoryManager.ConsumablesInBag.Count; k++)
                    {
                        if(inventoryManager.ConsumablesInBag[k]._ItemName == ActionsList[(pageNo * 10) + i]._Name)
                        {
                            newComponentList[i].selectedAction = ActionsList[(pageNo * 10) + i];
                            newComponentList[i].cmpName.text = newComponentList[i].selectedAction._Name;

                            int j = i;
                            newComponentList[i].cmpButton.onClick.RemoveAllListeners();
                            newComponentList[i].cmpButton.onClick.AddListener(delegate { SelectListAction(newComponentList[j].selectedAction); });
                        }
                    }
                }
            }
        }
    }


    public void ToggleTactics(TacticsModuleContainer thisContainer, Tactic tactic)
    {
        if(tactic._Condition == null || tactic._Action == null)
        {
            tactic.isTurnedOn = false;
            thisContainer.onToggle.text = "Off";
            return;
        }
        tactic.isTurnedOn = !tactic.isTurnedOn;
        thisContainer.onToggle.text = tactic.isTurnedOn ? "On" : "Off";
    }
    #region Swapping Conditions
    public void SelectTacticsCnd(TacticsModuleContainer thisContainer, Tactic tactic)
    {
        // CLICK BUTTON OF CND U WANT TO CHANGE
        // Designate this Cnd Space to be swapped with a new Cnd, or if double clicked, reset
        if (!listCoroutineRunning)
        {
            if (tacticCndToChange != tactic)
            {
                tacticCndToChange = tactic;
                currentContainer = thisContainer;
                tacticActionToChange = null;
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
    public void SelectListCnd(Condition cnd)
    {
        cndToBecome = cnd;
        SwapCnds();
        currentContainer = null;
        tacticCndToChange = null;
        StartCoroutine(ComponentListClose());
    }
    private void SwapCnds()
    {
        tacticCndToChange._Condition = cndToBecome;
        currentContainer.condition.text = tacticCndToChange._Condition.name;
    }
    #endregion
    #region Swapping Actions
    public void SelectTacticsAction(TacticsModuleContainer thisContainer, Tactic tactic)
    {
        // CLICK BUTTON OF ACTION U WANT TO CHANGE
        // Designate this Action Space to be swapped with a new Action, or if double clicked, reset
        if (!listCoroutineRunning)
        {
            if (tacticActionToChange != tactic)
            {
                tacticActionToChange = tactic;
                currentContainer = thisContainer;
                tacticCndToChange = null;
                StartCoroutine(ComponentListOpen());
                SetPages(false);
                SetActionList(0);
            }
            else
            {
                currentContainer = null;
                tacticActionToChange = null;
                StartCoroutine(ComponentListClose());
            }
        }
    }
    public void SelectListAction(Action action)
    {
        actionToBecome = action;
        SwapActions();
        currentContainer = null;
        tacticActionToChange = null;
        StartCoroutine(ComponentListClose());
    }
    private void SwapActions()
    {
        tacticActionToChange._Action = actionToBecome;
        currentContainer.action.text = tacticActionToChange._Action._Name;
    }
    #endregion
}
