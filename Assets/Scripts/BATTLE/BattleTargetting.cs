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

    public GameObject targetPanel;
    public List<Button> Targets;  //1, 2, 3, 4
    public BattleUIController BUC;

    private CharacterCircuit chosenTarget;

    // METHODS

    public void ChangeButtonNames()
    {
        foreach(Button a in Targets)
        {
            a.GetComponentInChildren<TextMeshProUGUI>().text = "";
            a.gameObject.SetActive(false);
        }
        for(int i = 0; i < BattleStateMachine._EnemiesActive.Count; i++)
        {
            Targets[i].gameObject.SetActive(true);
            // Change the name of the buttons to represent the targets
            // Set the buttons to do stuff when pressed (Close panel, action > enemy, Hero UI Change)
            Targets[i].GetComponentInChildren<TextMeshProUGUI>().text = BattleStateMachine._EnemiesActive[i].charName;
            chosenTarget = BattleStateMachine._EnemiesActive[i];

            Targets[i].onClick.AddListener(TurnOffPanel);
            Targets[i].onClick.AddListener(SetHeroAction);
        }
    }

    private void TurnOffPanel()
    {
        targetPanel.SetActive(false);
    }
    private void SetHeroAction()
    {
        BUC.CurrentHero.myTacticController.ActionIsInputted = true;
        BUC.CurrentHero.myTacticController.ChosenAction = BUC.CurrentHero._BasicAttack;
        BUC.CurrentHero.myTacticController.ChosenTarget = chosenTarget;
    }
}
