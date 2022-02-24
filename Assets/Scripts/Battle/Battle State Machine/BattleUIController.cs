using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BattleUIController : MonoBehaviour
{
    // VARIABLES
    [SerializeField]
    private BattleTargetting battleTargetting;
    public Transform heroContainer;

    public GameObject heroUIPrefab;
    public GameObject enemyUIPrefab;

    public List<HeroExtension> heroData;
    public List<HeroPrefabUIData> heroUIData = new();

    public List<EnemyExtension> enemyData;
    public List<EnemyPrefabUIData> enemyUIData = new();

    [SerializeField]
    internal HeroExtension CurrentHero; // This is who is being referenced in the Command Panel
    private PlayerControls playerControls;

    // UPDATES
    private void Awake()
    {
        playerControls = new PlayerControls();
    }
    private void Start()
    {
        StartUIData();
        CurrentHero = heroData[0];
    }
    private void Update()
    {
        switch (BattleStateMachine._CombatState)
        {
            case CombatState.ACTIVE:
                SetUIData();
                break;
        }
    }
    private void OnEnable()
    {
        playerControls.Enable();
        playerControls.BattleMap.HeroSwitch.performed += SwitchToNextHero;
    }
    private void OnDisable()
    {
        playerControls.Disable();
        playerControls.BattleMap.HeroSwitch.performed -= SwitchToNextHero;
    }

    // METHODS
    private void SwitchToNextHero(InputAction.CallbackContext context)
    {
        int i = (int)context.ReadValue<float>();
        int j = heroData.IndexOf(CurrentHero) + i;

        if(j < 0)
        {
            j = heroData.Count - 1;
        }
        else if(j >= heroData.Count)
        {
            j = 0;
        }
        CurrentHero = heroData[j];
        battleTargetting.ResetCommands();
        Debug.Log("'New hero selected is: " + heroData[j].charName);
    }


    private void StartUIData()
    {
        for (int i = 0; i < heroData.Count; i++)
        {
            heroUIData[i].atbBar.fillAmount = heroData[i]._ActionChargeAmount;

            heroUIData[i].healthBar.fillAmount = (float)heroData[i]._CurrentHP / heroData[i].MaxHP;

            heroUIData[i].manaBar.fillAmount = (float)heroData[i]._CurrentMP / heroData[i].MaxMP;

            heroUIData[i].health.text = heroData[i]._CurrentHP.ToString() + " / " +
                                        heroData[i].MaxHP.ToString();

            heroUIData[i].mana.text = heroData[i]._CurrentMP.ToString() + " / " +
                                      heroData[i].MaxMP.ToString();
        }
    }                   // Hero Info in Battle
    private void SetUIData()
    {
        for (int i = 0; i < heroData.Count; i++)
        {
            if (heroData[i].myTacticController.ChosenAction != null)
            {
                CharacterTemplate tempToUse;
                switch (heroData[i].myTacticController.ChosenTarget.myType)
                {
                    case BattleCharacterController.ControllerType.HERO:
                        {
                            BattleHeroController a = heroData[i].myTacticController.ChosenTarget as BattleHeroController;
                            tempToUse = a.myHero;
                            break;
                        }

                    default:
                        {
                            BattleEnemyController a = heroData[i].myTacticController.ChosenTarget as BattleEnemyController;
                            tempToUse = a.myEnemy;
                            break;
                        }
                }
                heroUIData[i].action.text = heroData[i].myTacticController.ChosenAction._Name + " > " +
                                            tempToUse.charName;
            }
            else
            {
                heroUIData[i].action.text = "";
            }
            heroUIData[i].atbBar.fillAmount = heroData[i]._ActionChargeAmount / 100;

            heroUIData[i].healthBar.fillAmount = (float)heroData[i]._CurrentHP / heroData[i].MaxHP;

            heroUIData[i].manaBar.fillAmount = (float)heroData[i]._CurrentMP / heroData[i].MaxMP;

            heroUIData[i].health.text = heroData[i]._CurrentHP.ToString() + "/" +
                                        heroData[i].MaxHP.ToString() + " HP";

            heroUIData[i].mana.text = heroData[i]._CurrentMP.ToString() + "/" +
                                      heroData[i].MaxMP.ToString() + " MP";
        }
        for (int i = 0; i < enemyData.Count; i++)
        {
            enemyUIData[i].healthBar.fillAmount = (float)enemyData[i]._CurrentHP / enemyData[i].MaxHP;
            enemyUIData[i].health.text = enemyData[i]._CurrentHP.ToString();
        }
    }                     // Updating Hero Info in Battle
    public void CreateHeroUI(HeroExtension hero)
    {
        heroData.Add(hero);

        GameObject heroUI = Instantiate(heroUIPrefab, heroContainer);
        heroUI.name = hero.charName + " UI";
        heroUI.GetComponent<HeroPrefabUIData>().characterIcon.sprite = hero.charPortrait;
        heroUIData.Add(heroUI.GetComponent<HeroPrefabUIData>());
    }
    public void CreateEnemyUI(EnemyExtension enemy, Transform enemyModel)
    {
        enemyData.Add(enemy);

        GameObject enemyUI = Instantiate(enemyUIPrefab, 
                                         enemyModel.Find("Battle Display").position, 
                                         enemyModel.Find("Battle Display").rotation,
                                         enemyModel.Find("Battle Display"));
        enemyUI.name = enemy.charName + " UI";

        EnemyPrefabUIData data = enemyUI.GetComponent<EnemyPrefabUIData>();
        data.identity.text = enemy.name;
        data.healthBar.fillAmount = enemy.MaxHP;

        enemyUIData.Add(data);
    }
}
