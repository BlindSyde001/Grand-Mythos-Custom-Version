using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

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

    private PlayerControls playerControls;

    [SerializeField]
    internal HeroExtension CurrentHero; // This is who is being referenced in the Command Panel
    public HeroPrefabUIData currentHeroUI;
    #region Actions UI
    [SerializeField]
    internal List<GameObject> singleActions;
    [SerializeField]
    internal List<GameObject> doubleActions;
    [SerializeField]
    internal List<GameObject> tripleActions;
    [SerializeField]
    internal GameObject quadAction;
    #endregion

    // UPDATES
    private void Awake()
    {
        playerControls = new PlayerControls();
    }
    private void Start()
    {
        CurrentHero = BattleStateMachine._HeroesActive[0].myHero;
        StartUIData();
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
        SetCurrentHeroData();
        int j = 0;
        for (int i = 0; i < heroData.Count; i++)
        {
            if (heroData[i] == CurrentHero)
            if (heroData[i] != CurrentHero)
            {
                heroUIData[j].gameObject.SetActive(true);
                heroUIData[j].characterIcon.sprite = heroData[i].charPortrait;
                heroUIData[j].atbBar.fillAmount = heroData[i]._ActionChargeAmount;
                heroUIData[j].healthBar.fillAmount = (float)heroData[i]._CurrentHP / heroData[i].MaxHP;
                heroUIData[j].health.text = heroData[i]._CurrentHP.ToString() + " / " +
                                            heroData[i].MaxHP.ToString();
                j++;
            }
        }
    }                   // Hero Info in Battle
    private void SetCurrentHeroData()
    {
        currentHeroUI.characterIcon.sprite = CurrentHero.charPortrait;
        currentHeroUI.atbBar.fillAmount = CurrentHero._ActionChargeAmount / 100;
        currentHeroUI.healthBar.fillAmount = (float)CurrentHero._CurrentHP / CurrentHero.MaxHP;
        currentHeroUI.health.text = CurrentHero._CurrentHP.ToString();
    }
    private void SetUIData()
    {
        SetCurrentHeroData();
        SetCurrentHeroActionsUI();
        int j = 0;
        for (int i = 0; i < heroData.Count; i++)
        {
            if (heroData[i] != CurrentHero)
            {
                heroUIData[j].characterIcon.sprite = heroData[i].charPortrait;
                heroUIData[j].atbBar.fillAmount = heroData[i]._ActionChargeAmount / 100;
                heroUIData[j].healthBar.fillAmount = (float)heroData[i]._CurrentHP / heroData[i].MaxHP;
                heroUIData[j].health.text = heroData[i]._CurrentHP.ToString();
                j++;
            }
        }
        for (int i = 0; i < enemyData.Count; i++)
        {
            enemyUIData[i].healthBar.fillAmount = (float)enemyData[i]._CurrentHP / enemyData[i].MaxHP;
            enemyUIData[i].health.text = enemyData[i]._CurrentHP.ToString();
        }
    }                     // Updating Hero Info in Battle

    private void SetCurrentHeroActionsUI()
    {
        foreach(GameObject a in singleActions)
        {
            a.SetActive(false);
        }
        foreach (GameObject a in doubleActions)
        {
            a.SetActive(false);
        }
        foreach (GameObject a in tripleActions)
        {
            a.SetActive(false);
        }
        quadAction.SetActive(false);
        for (int i = 0; i < CurrentHero.myTacticController.ChosenActions.Count; i++)
        {
            if (CurrentHero.myTacticController.ChosenActions[i] != null)
            {
                switch (CurrentHero.myTacticController.ChosenActions[i]._SegmentCost)
                {
                    case 1:
                        singleActions[i].SetActive(true);
                        singleActions[i].GetComponentInChildren<TextMeshProUGUI>().text = CurrentHero.myTacticController.ChosenActions[i].Name;
                        break;
                    case 2:
                        doubleActions[i].SetActive(true);
                        doubleActions[i].GetComponentInChildren<TextMeshProUGUI>().text = CurrentHero.myTacticController.ChosenActions[i].Name;
                        break;
                    case 3:
                        tripleActions[i].SetActive(true);
                        tripleActions[i].GetComponentInChildren<TextMeshProUGUI>().text = CurrentHero.myTacticController.ChosenActions[i].Name;
                        break;
                    case 4:
                        quadAction.SetActive(true);
                        quadAction.GetComponent<TextMeshProUGUI>().text = CurrentHero.myTacticController.ChosenActions[i].Name;
                        break;
                }
            }
        }
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
    public void ToggleTactics()
    {
        CurrentHero.myTacticController.tacticsAreActive = !CurrentHero.myTacticController.tacticsAreActive;
    }
}