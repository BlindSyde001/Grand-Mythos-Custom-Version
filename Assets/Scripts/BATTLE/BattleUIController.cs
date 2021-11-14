using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUIController : MonoBehaviour
{
    // VARIABLES
    private BattleStateMachine BSM;
    public Transform heroContainer;

    public GameObject heroUIPrefab;
    public GameObject enemyUIPrefab;

    public List<HeroExtension> heroData;
    public List<HeroPrefabData> heroUIData = new List<HeroPrefabData>();

    public List<EnemyExtension> enemyData;
    public List<EnemyPrefabData> enemyUIData = new List<EnemyPrefabData>();

    public HeroExtension CurrentHero; // This is who is being referenced in the Command Panel

    // UPDATES
    private void Awake()
    {
        BSM = FindObjectOfType<BattleStateMachine>();
    }
    private void Start()
    {
        StartUIData();
        CurrentHero = heroData[0];
    }

    private void Update()
    {
        switch (BSM._BattleState)
        {
            case BattleState.ACTIVE:
                SetUIData();
                break;
        }
    }

    // METHODS
    private void StartUIData()
    {
        for (int i = 0; i < heroData.Count; i++)
        {
            heroUIData[i].atbBar.value = heroData[i]._ActionChargeAmount;

            heroUIData[i].health.text = heroData[i]._CurrentHP.ToString() + "/" +
                                        heroData[i].MaxHP.ToString() + " HP";

            heroUIData[i].mana.text = heroData[i]._CurrentMP.ToString() + "/" +
                                      heroData[i].MaxMP.ToString() + " HP";
        }
    }
    private void SetUIData()
    {
        for (int i = 0; i < heroData.Count; i++)
        {
            if (heroData[i].myTacticController.ChosenAction != null)
            {
                heroUIData[i].nextAction.text = heroData[i].myTacticController.ChosenAction._Name + " > " +
                                                heroData[i].myTacticController.ChosenTarget.charName;
            }
            else
            {
                heroUIData[i].nextAction.text = "";
            }
            heroUIData[i].atbBar.value = heroData[i]._ActionChargeAmount;

            heroUIData[i].health.text = heroData[i]._CurrentHP.ToString() + "/" +
                                        heroData[i].MaxHP.ToString() + " HP";

            heroUIData[i].mana.text = heroData[i]._CurrentMP.ToString() + "/" +
                                      heroData[i].MaxMP.ToString() + " HP";
        }
        for (int i = 0; i < enemyData.Count; i++)
        {
            enemyUIData[i].healthBar.value = enemyData[i]._CurrentHP;
            enemyUIData[i].health.text = enemyData[i]._CurrentHP.ToString();
        }
    }
    public void CreateHeroUI(HeroExtension hero)
    {
        heroData.Add(hero);

        GameObject heroUI = Instantiate(heroUIPrefab, heroContainer);
        heroUI.name = hero.charName + " UI";

        HeroPrefabData data = new HeroPrefabData();

        data.nextAction = heroUI.transform.Find("Action Name").GetComponent<TextMeshProUGUI>();
        data.atbBar = heroUI.transform.GetComponentInChildren<Slider>();
        data.health = heroUI.transform.Find("HP").GetComponent<TextMeshProUGUI>();
        data.mana = heroUI.transform.Find("MP").GetComponent<TextMeshProUGUI>();
        heroUIData.Add(data);
    }
    public void CreateEnemyUI(EnemyExtension enemy, Transform enemyModel)
    {
        enemyData.Add(enemy);

        GameObject enemyUI = Instantiate(enemyUIPrefab, 
                                         enemyModel.Find("Battle Display").position, 
                                         enemyModel.Find("Battle Display").rotation,
                                         enemyModel.Find("Battle Display"));
        enemyUI.name = enemy.charName + " UI";

        EnemyPrefabData data = new EnemyPrefabData();
        data.enemyName = enemyUI.transform.Find("Enemy Name").GetComponent<TextMeshProUGUI>();
        data.enemyName.text = enemy.name;

        data.healthBar = enemyUI.transform.GetComponentInChildren<Slider>();
        data.healthBar.maxValue = enemy.MaxHP;
        data.health = enemyUI.transform.Find("HP").GetComponent<TextMeshProUGUI>();

        enemyUIData.Add(data);
    }
}

[System.Serializable]
public class HeroPrefabData
{
    public TextMeshProUGUI nextAction;
    public Slider atbBar;
    public TextMeshProUGUI health;
    public TextMeshProUGUI mana;
}

[System.Serializable]
public class EnemyPrefabData
{
    public TextMeshProUGUI enemyName;
    public TextMeshProUGUI health;
    public Slider healthBar;
}
