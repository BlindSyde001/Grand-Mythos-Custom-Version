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
        CurrentHero = BattleStateMachine._HeroesActive[0].myHero;
    }
    private void Update()
    {
        SetUIData();
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
        if (BattleStateMachine._CombatState == CombatState.ACTIVE)
        {
            int i = (int)context.ReadValue<float>();
            int j = BattleStateMachine._HeroesActive.IndexOf(CurrentHero.myBattleHeroController) + i;

            if (j < 0)
            {
                j = BattleStateMachine._HeroesActive.Count - 1;
            }
            else if (j >= BattleStateMachine._HeroesActive.Count)
            {
                j = 0;
            }
            CurrentHero = BattleStateMachine._HeroesActive[j].myHero;
            battleTargetting.ResetCommands();
        }
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
            if (heroData[i].myTacticController.ChosenAction != null && BattleStateMachine.CheckStateOfPlay())
            {
                //if (heroData[i].myTacticController.ChosenTarget != null)
                //{
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
                    heroUIData[i].action.text = heroData[i].myTacticController.ChosenAction.Name + " > " +
                                                tempToUse.charName;
                //}
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
    public void AttachHeroUIData(HeroExtension hero, int i)
    {
        heroData.Add(hero);
        heroUIData[i].gameObject.SetActive(true);
        heroUIData[i].name = hero.charName + "UI";
        heroUIData[i].characterIcon.sprite = hero.charPortrait;
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
