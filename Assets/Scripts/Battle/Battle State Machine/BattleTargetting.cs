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
    private BattleUIController battlUIController;
    private InventoryManager inventoryManager;

    public GameObject mainCommandsPanel;
    public List<Button> mainCommands;

    public GameObject actionsPanel;
    public List<BattleActionsListContainer> actions;

    public GameObject targetPanel;
    public List<BattleTargetsListContainer> Targets;  //1, 2, 3, 4

    private bool skillsOpen;
    private bool itemsOpen;

    private Action chosenAction;
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
        actionsPanel.SetActive(true);
        targetPanel.SetActive(false);
        skillsOpen = true;
        itemsOpen = false;

        // Add the Data of the Hero's skills onto each button
        for(int i = 0; i < battlUIController.CurrentHero._AvailableActions.Count; i++)
        {
            actions[i].myName.text = "";
            int j = i;
            actions[i].myAction = battlUIController.CurrentHero._AvailableActions[i];
            actions[i].myName.text = battlUIController.CurrentHero._AvailableActions[i].Name;
            actions[i].myButton.onClick.AddListener(delegate {SetAction(actions[j].myAction); });
        }
        actions[0].GetComponent<Button>().Select();
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
            actions[i].myName.text = consumable._ItemName;
            actions[i].myButton.onClick.AddListener(delegate { SetAction(actions[j].myAction); });
        }
        actions[0].GetComponent<Button>().Select();
    }
    
    public void SetAction(Action action)
    {
        chosenAction = action;
        OpenTargetList(action.ActionEffect == ActionEffect.HEAL? 1 : 0);
        Targets[0].GetComponent<Button>().Select();
    }
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
                foreach (BattleHeroController a in BattleStateMachine._HeroesActive)
                {
                    allHeroes.Add(a.myHero);
                }
                foreach(BattleHeroController a in BattleStateMachine._HeroesDowned)
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

    public void ChooseTarget(BattleCharacterController target)
    {
        chosenTarget = target;
        targetPanel.SetActive(false);
        InputHeroCommand(chosenAction, chosenTarget);
        mainCommands[0].Select();
    }
    private void InputHeroCommand(Action action, BattleCharacterController target)
    {
        battlUIController.CurrentHero.myTacticController.ActionIsInputted = true;
        battlUIController.CurrentHero.myTacticController.ChosenAction = action;
        battlUIController.CurrentHero.myTacticController.ChosenTarget = target;
        if(action.ActionType == ActionType.ITEM)
        {
            Consumable consumable = GameManager._instance._ConsumablesDatabase.Find(x => x.myAction == action);
            ItemCapsule itemCapsule = inventoryManager.ConsumablesInBag.Find(x => x.thisItem == consumable);

            inventoryManager.RemoveFromInventory(itemCapsule);
        }
    }
}