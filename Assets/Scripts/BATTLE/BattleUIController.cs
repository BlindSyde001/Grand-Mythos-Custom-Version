using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUIController : MonoBehaviour
{
    private BattleStateMachine BSM;
    public Transform heroContainer;
    public GameObject heroPrefab;

    public List<HeroExtension> characterData;
    public List<PrefabData> heroUIData = new List<PrefabData>();

    private void Awake()
    {
        BSM = FindObjectOfType<BattleStateMachine>();
    }

    private void Update()
    {
        SetUIData();
    }

    private void SetUIData()
    {
        for (int i = 0; i < characterData.Count; i++)
        {
            if (characterData[i].myTacticController.nextAction != null)
            {
                heroUIData[i].nextAction.text = characterData[i].myTacticController.nextAction._Name;
            }
            else
            {
                heroUIData[i].nextAction.text = "";
            }
            heroUIData[i].atbBar.value = characterData[i]._ActionChargeAmount;

            heroUIData[i].health.text = characterData[i]._CurrentHP.ToString() + "/" +
                                        characterData[i].MaxHP.ToString() + " HP";

            heroUIData[i].mana.text = characterData[i]._CurrentMP.ToString() + "/" +
                                      characterData[i].MaxMP.ToString() + " HP";
        }
    }
    public void CreateHeroUI(HeroExtension hero)
    {
        characterData.Add(hero);

        GameObject heroUI = Instantiate(heroPrefab, heroContainer);
        heroUI.name = hero.charName + " UI";

        PrefabData data = new PrefabData();

        data.nextAction = heroUI.transform.Find("Action Name").GetComponent<TextMeshProUGUI>();
        data.atbBar = heroUI.transform.GetComponentInChildren<Slider>();
        data.health = heroUI.transform.Find("HP").GetComponent<TextMeshProUGUI>();
        data.mana = heroUI.transform.Find("MP").GetComponent<TextMeshProUGUI>();
        heroUIData.Add(data);
    }
}

[System.Serializable]
public class PrefabData
{
    public TextMeshProUGUI nextAction;
    public Slider atbBar;
    public TextMeshProUGUI health;
    public TextMeshProUGUI mana;
}