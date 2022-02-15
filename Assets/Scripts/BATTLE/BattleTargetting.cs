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

    public BattleUIController BUC;

    public GameObject actionsPanel;
    public List<BattleActionsListContainer> actions;

    public GameObject targetPanel;
    public List<BattleTargetsListContainer> Targets;  //1, 2, 3, 4

    private bool skillsOpen;
    private bool itemsOpen;

    private Action chosenAction;
    private CharacterCircuit chosenTarget;

    // METHODS
    public void OpenSkillsList()
    {
        if(actionsPanel.activeSelf == true && skillsOpen)
        {
            actionsPanel.SetActive(false);
            skillsOpen = false;
            return;
        }
        actionsPanel.SetActive(true);
        skillsOpen = true;
        itemsOpen = false;

        for(int i = 0; i < BUC.CurrentHero._AvailableActions.Count; i++)
        {
            actions[i].myName.text = "";
            int j = i;
            actions[i].myAction = BUC.CurrentHero._AvailableActions[i];
            actions[i].myName.text = BUC.CurrentHero._AvailableActions[i]._Name;
            actions[i].myButton.onClick.AddListener(delegate {SetAction(actions[j].myAction); });
        }
    }
    public void OpenItemsList()
    {
        if (actionsPanel.activeSelf == true && itemsOpen)
        {
            actionsPanel.SetActive(false);
            itemsOpen = false;
            return;
        }
        actionsPanel.SetActive(true);
        skillsOpen = false;
        itemsOpen = true;

        foreach(BattleActionsListContainer a in actions)
        {
            a.myName.text = "";
            a.myAction = null;
            a.myButton.onClick.RemoveAllListeners();
        }
        for (int i = 0; i < InventoryManager._instance.ConsumablesInBag.Count; i++)
        {
            int j = i;
            actions[i].myAction = InventoryManager._instance.ConsumablesInBag[i].myAction;
            actions[i].myName.text = InventoryManager._instance.ConsumablesInBag[i]._ItemName;
            actions[i].myButton.onClick.AddListener(delegate { SetAction(actions[j].myAction); });
        }
    }
    
    public void SetAction(Action action)
    {
        chosenAction = action;
        OpenTargetList(action._ActionEffect == ActionEffect.HEAL? 1 : 0);
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
                    Targets[i].myName.text = BattleStateMachine._EnemiesActive[i].charName;
                    Targets[i].myButton.onClick.AddListener(delegate { ChooseTarget(Targets[j].myTarget); });
                }
                break;

            case 1:
                // hero list
                List<HeroExtension> allHeroes = new();
                foreach (HeroExtension a in GameManager._instance._PartyMembersActive)
                {
                    allHeroes.Add(a);
                }
                foreach(HeroExtension a in GameManager._instance._PartyMembersDowned)
                {
                    allHeroes.Add(a);
                }

                for(int i = 0; i < allHeroes.Count ; i++)
                {
                    Targets[i].myName.text = "";
                    int j = i;
                    Targets[i].myTarget = allHeroes[i];
                    Targets[i].myName.text = allHeroes[i].charName;
                    Targets[i].myButton.onClick.AddListener(delegate {ChooseTarget(Targets[j].myTarget); });
                }
                break;
        }
    }

    public void ChooseTarget(CharacterCircuit target)
    {
        chosenTarget = target;
        targetPanel.SetActive(false);
        InputHeroCommand(chosenAction, chosenTarget);
    }
    private void InputHeroCommand(Action action, CharacterCircuit target)
    {
        BUC.CurrentHero.myTacticController.ActionIsInputted = true;
        BUC.CurrentHero.myTacticController.ChosenAction = action;
        BUC.CurrentHero.myTacticController.ChosenTarget = target;
    }
}
