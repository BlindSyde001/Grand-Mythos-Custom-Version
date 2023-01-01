using System.Collections;
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
    public List<HeroPrefabUIData> heroUIData;

    public List<EnemyExtension> enemyData;
    public List<EnemyPrefabUIData> enemyUIData;

    private PlayerControls playerControls;

    [SerializeField]
    internal HeroExtension ChosenHero; // This is who is being referenced in the Command Panel
    [SerializeField]
    internal List<Action> ChosenHeroActions = new();
    public HeroPrefabUIData ChosenHeroUI;
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
        ChosenHero = BattleStateMachine._HeroesActive[0].myHero;
        SetOtherHeroUIData();
    }
    private void Update()
    {
        SetOtherHeroUIData();
        if (BattleStateMachine.CheckStateOfPlay())
        {
            for (int i = 0; i < ChosenHero.myTacticController.ChosenActions.Count; i++)
            {
                if (ChosenHero.myTacticController.ChosenActions[i] != null && ChosenHero.myTacticController.ChosenActions[i] != ChosenHeroActions[i])
                {
                    StartCoroutine(AddHeroActionUI(ChosenHero.myTacticController.ChosenActions[i], i));
                }
            }
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
    #region Player Command's Methods
    private void SwitchToNextHero(InputAction.CallbackContext context)
    {
        if (BattleStateMachine._CombatState == CombatState.ACTIVE)
        {
            int i = (int)context.ReadValue<float>();
            int j = BattleStateMachine._HeroesActive.IndexOf(ChosenHero.myBattleHeroController) + i;

            if (j < 0)
            {
                j = BattleStateMachine._HeroesActive.Count - 1;
            }
            else if (j >= BattleStateMachine._HeroesActive.Count)
            {
                j = 0;
            }

            ResetCurrentHeroActionUI();
            battleTargetting.ResetCommands();
            ChosenHero = BattleStateMachine._HeroesActive[j].myHero;
            SetChosenHeroActions();
        }
    }     // Hero Info in Battle
    public void ToggleTactics()
    {
        ChosenHero.myTacticController.tacticsAreActive = !ChosenHero.myTacticController.tacticsAreActive;
    }
    #endregion
    #region Setting UI Data
    private void SetCurrentHeroUIData()
    {
        ChosenHeroUI.characterIcon.sprite = ChosenHero.charPortrait;
        ChosenHeroUI.atbBar.fillAmount = ChosenHero._ActionChargeAmount / 100;
        ChosenHeroUI.healthBar.fillAmount = (float)ChosenHero._CurrentHP / ChosenHero.MaxHP;
        ChosenHeroUI.health.text = ChosenHero._CurrentHP.ToString();
    }
    private void SetOtherHeroUIData()
    {
        SetCurrentHeroUIData();
        int j = 0;
        for (int i = 0; i < heroData.Count; i++)
        {
            if (heroData[i] != ChosenHero)
            {
                heroUIData[j].gameObject.SetActive(true);
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
    }                                      // Updating Hero Info in Battle
    #endregion
    #region Current Hero Actions
    private void SetChosenHeroActions()
    {
        if(ChosenHero.myTacticController.ChosenActions != null)
        {
            ChosenHeroActions = new(ChosenHero.myTacticController.ChosenActions);
        }
    }
    private void ResetCurrentHeroActionUI()
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
    }

    public IEnumerator AddHeroActionUI(Action action, int uiNumber)
    {
        switch (action._SegmentCost)
        {
            case 1:
                singleActions[uiNumber].SetActive(true);
                singleActions[uiNumber].GetComponentInChildren<TextMeshProUGUI>().text = action.Name;
                break;
            case 2:
                doubleActions[uiNumber].SetActive(true);
                doubleActions[uiNumber].GetComponentInChildren<TextMeshProUGUI>().text = action.Name;
                break;
            case 3:
                tripleActions[uiNumber].SetActive(true);
                tripleActions[uiNumber].GetComponentInChildren<TextMeshProUGUI>().text = action.Name;
                break;
            case 4:
                quadAction.SetActive(true);
                quadAction.GetComponent<TextMeshProUGUI>().text = action.Name;
                break;
        }
        yield return null;
    }
    public IEnumerator RemoveHeroActionUI(Action action, int uiNumber)
    {
        Debug.Log("Remove this Action from UI!");
        yield return null;
    }
    #endregion
}