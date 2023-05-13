using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleTargetting : MonoBehaviour
{
    // 1. When opened, buttons represent the enemies in the fight
    // 2. Buttons, when pressed will do the action from the Hero to the target
    // 3. Buttons, when pressed, will close the targetting panel
    // 4. Buttons, when pressed will affect the Hero UI such that it reads: ( [Action] > [Target] )

    // VARIABLES
    [SerializeField]
    private BattleUIController battleUIController;
    private InventoryManager inventoryManager;

    public GameObject mainCommandsPanel;
    public List<Button> mainCommands;

    public GameObject actionsPanel;
    public List<BattleActionsListContainer> actions;

    public GameObject targetPanel;
    public List<BattleTargetsListContainer> Targets;  //1, 2, 3, 4
    internal List<BattleHeroModelController> heroTargets = new();

    private bool skillsOpen;
    private bool itemsOpen;

    private List<Action> chosenActions = new();
    private BattleCharacterController chosenTarget;

    // UPDATES
    private void Awake()
    {
        inventoryManager = InventoryManager._instance;
    }

    // METHODS
    public void ResetCommands()
    {
        actionsPanel.SetActive(false);
        targetPanel.SetActive(false);
        skillsOpen = false;
        itemsOpen = false;
        mainCommands[0].Select();
    }

    #region STEP 1: Navigating your Actions
    public void OpenSkillsList()
    {
        // Panel toggling stuff
        if(actionsPanel.activeSelf == true && skillsOpen)
        {
            actionsPanel.SetActive(false);
            skillsOpen = false;
            mainCommands[0].Select();
            return;
        }
        if (BattleStateMachine._CombatState != CombatState.END && BattleStateMachine._CombatState != CombatState.START)
        {
            actionsPanel.SetActive(true);
            targetPanel.SetActive(false);
            skillsOpen = true;
            itemsOpen = false;

            // Add the Data of the Hero's skills onto each button
            for (int i = 0; i < battleUIController.ChosenHero._AvailableActions.Count; i++)
            {
                actions[i].myName.text = "";
                int j = i;
                actions[i].myAction = battleUIController.ChosenHero._AvailableActions[i];
                actions[i].myName.text = battleUIController.ChosenHero._AvailableActions[i].Name;
                actions[i].myButton.onClick.AddListener(delegate { SetAction(actions[j].myAction); });
            }
            actions[0].GetComponent<Button>().Select();
        }
    }
    public void OpenItemsList()
    {
        // Panel toggling stuff
        if (actionsPanel.activeSelf == true && itemsOpen)
        {
            actionsPanel.SetActive(false);
            itemsOpen = false;
            mainCommands[0].Select();
            return;
        }
        if (BattleStateMachine._CombatState != CombatState.END && BattleStateMachine._CombatState != CombatState.START)
        {
            actionsPanel.SetActive(true);
            targetPanel.SetActive(false);
            skillsOpen = false;
            itemsOpen = true;

            // Reset Button, then add Usable Items data onto them
            foreach (BattleActionsListContainer a in actions)
            {
                a.myName.text = "";
                a.myAction = null;
                a.myButton.onClick.RemoveAllListeners();
            }
            for (int i = 0; i < inventoryManager.ConsumablesInBag.Count; i++)
            {
                Consumable consumable = (Consumable)inventoryManager.ConsumablesInBag[i].thisItem;
                int j = i;
                actions[i].myAction = consumable.myAction;
                actions[i].myName.text = consumable._ItemName + " x " + inventoryManager.ConsumablesInBag[i].ItemAmount;
                actions[i].myButton.onClick.AddListener(delegate { SetAction(actions[j].myAction); });
            }
            actions[0].GetComponent<Button>().Select();
        }
    }
    #endregion
    #region STEP 2: Setting your Actions
    public void SetAction(Action action)
    {
        if (BattleStateMachine._CombatState != CombatState.START && BattleStateMachine._CombatState != CombatState.END)
        {   // Set Actions, then, if the Action Segments are full, go Select the Target
            if (ActionSegmentsAreFilled(action))
            {
                mainCommandsPanel.SetActive(false);
                OpenTargetList(action.ActionEffect == ActionEffect.HEAL ? 1 : 0);
                Targets[0].GetComponent<Button>().Select();
            }
        }
    }

    private bool ActionSegmentsAreFilled(Action action)
    {
        int allowance = battleUIController.ChosenHero.myTacticController.ActionAllowance;
        switch(allowance + action._SegmentCost)
        {
            case > 4:
                return true;

            case 4:
                AddActionSegment(action, allowance);
                return true;

            case < 4:
                AddActionSegment(action, allowance);
                return false;
        }
    }
    private void AddActionSegment(Action action, int allowance)
    {
        switch (action._SegmentCost)
        {
            case 1:
                battleUIController.ChosenHero.myTacticController.ActionAllowance += action._SegmentCost;
                chosenActions.Add(action);
                StartCoroutine(battleUIController.AddHeroActionUI(action, allowance));
                break;

            case 2:
                battleUIController.ChosenHero.myTacticController.ActionAllowance += action._SegmentCost;
                chosenActions.Add(action);
                StartCoroutine(battleUIController.AddHeroActionUI(action, allowance));
                break;

            case 3:
                battleUIController.ChosenHero.myTacticController.ActionAllowance += action._SegmentCost;
                chosenActions.Add(action);
                StartCoroutine(battleUIController.AddHeroActionUI(action, allowance));
                break;

            case 4:
                battleUIController.ChosenHero.myTacticController.ActionAllowance += action._SegmentCost;
                chosenActions.Add(action);
                StartCoroutine(battleUIController.AddHeroActionUI(action, allowance));
                break;
        }
    }
    #endregion
    #region STEP 3: Setting your Target
    public void OpenTargetList(int enemyOrHero)
    {
        actionsPanel.SetActive(false);
        skillsOpen = false;
        itemsOpen = false;

        targetPanel.SetActive(true);
        foreach(BattleTargetsListContainer a in Targets)
        {
            a.myName.text = "";
            a.myTarget = null;
            a.myButton.onClick.RemoveAllListeners();
        }
        switch(enemyOrHero)
        {
            case 0:
                // enemy list
                for(int i = 0; i < BattleStateMachine._EnemiesActive.Count; i++)
                {
                    int j = i;
                    Targets[i].myTarget = BattleStateMachine._EnemiesActive[i];
                    Targets[i].myName.text = BattleStateMachine._EnemiesActive[i].myEnemy.charName;
                    Targets[i].myButton.onClick.AddListener(delegate { ChooseTarget(Targets[j].myTarget); });
                }
                break;

            case 1:
                // hero list
                List<HeroExtension> allHeroes = new();
                foreach (BattleHeroModelController a in heroTargets)
                {
                    allHeroes.Add(a.myHero);
                }

                for(int i = 0; i < allHeroes.Count ; i++)
                {
                    Targets[i].myName.text = "";
                    int j = i;
                    Targets[i].myTarget = allHeroes[i].myBattleHeroController;
                    Targets[i].myName.text = allHeroes[i].charName;
                    Targets[i].myButton.onClick.AddListener(delegate {ChooseTarget(Targets[j].myTarget); });
                }
                break;
        }
    }
    public void ChooseTarget(BattleCharacterController target) // Select the Target, input an Action into the next Segment
    {
        chosenTarget = target;
        targetPanel.SetActive(false);
        InputHeroCommand(chosenActions, chosenTarget);
        //mainCommands[0].Select();
    }
    #endregion
    #region STEP 4: Input the Actions
    private void InputHeroCommand(List<Action> action, BattleCharacterController target)
    {
        battleUIController.ChosenHero.myTacticController.ManualActionInput = true;
        battleUIController.ChosenHero.myTacticController.ChosenTarget = target;
    }
    #endregion
}