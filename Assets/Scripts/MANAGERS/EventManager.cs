using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState {BATTLE, OVERWORLD, TITLE, CUTSCENE, MENU }
public enum CombatState {START, ACTIVE, WAIT, END}
public class EventManager : MonoBehaviour
{
    // VARIABLES
    public static EventManager _instance;

    [SerializeField]
    private GameManager gameManager;
    [SerializeField]
    private InputManager inputManager;
    [SerializeField]
    private SceneChangeManager sceneChangeManager;

    [SerializeField]
    internal GameState _GameState;

    public delegate void LoadNewScene();
    public static event LoadNewScene LoadingOverworld;
    public static event LoadNewScene LoadingBattle;
    public static event LoadNewScene LoadingTitle;

    public delegate void ChangeInGameState(GameState GS);
    public static event ChangeInGameState OnGameStateChange;

    // UPDATES
    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
        }
        else if(_instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        LoadingOverworld += LoadOverworld;
        LoadingBattle += LoadBattle;
        LoadingTitle += LoadTitle;

        OnGameStateChange += TitleState;
        OnGameStateChange += OverworldState;
        OnGameStateChange += BattleState;
        OnGameStateChange += CutsceneState;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        LoadingOverworld -= LoadOverworld;
        LoadingBattle -= LoadBattle;
        LoadingTitle -= LoadTitle;

        OnGameStateChange -= TitleState;
        OnGameStateChange -= OverworldState;
        OnGameStateChange -= BattleState;
        OnGameStateChange -= CutsceneState;
    }

    // METHODS
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch(scene.buildIndex)
        {
            case 0:
                SwitchGameState(GameState.TITLE);
                break;

            case 1:
                SwitchGameState(GameState.BATTLE);
                break;

            case > 1:
                SwitchGameState(GameState.OVERWORLD);
                break;
        }
    }

    #region GAMESTATE CHANGING
    public void SwitchGameState(GameState GS)
    {
        Debug.Log(GS);
        switch (GS)
        {
            case GameState.TITLE:
                OnGameStateChange(GameState.TITLE);
                break;

            case GameState.OVERWORLD:
                OnGameStateChange(GameState.OVERWORLD);
                break;

            case GameState.BATTLE:
                OnGameStateChange(GameState.BATTLE);
                break;

            case GameState.CUTSCENE:
                OnGameStateChange(GameState.CUTSCENE);
                break;

            case GameState.MENU:
                MenuState(GameState.MENU);
                break;
        }
    }

    private void TitleState(GameState GS)
    {
        if(GS == GameState.TITLE)
        {
            _GameState = GameState.TITLE;
            inputManager.playerInput.SwitchCurrentActionMap("Title Map");
        }
    }
    private void OverworldState(GameState GS)
    {
        if(GS == GameState.OVERWORLD)
        {
            _GameState = GameState.OVERWORLD;
            inputManager.playerInput.SwitchCurrentActionMap("Overworld Map");
        }
    }
    private void BattleState(GameState GS)
    {
        if(GS == GameState.BATTLE)
        {
            _GameState = GameState.BATTLE;
            inputManager.playerInput.SwitchCurrentActionMap("Battle Map");
        }
    }
    private void CutsceneState(GameState GS)
    {
        if(GS == GameState.CUTSCENE)
        {
            _GameState = GameState.CUTSCENE;
            inputManager.playerInput.SwitchCurrentActionMap("Cutscene Map");
        }
    }
    private void MenuState(GameState GS)
    {
        if(GS == GameState.MENU)
        {
            _GameState = GameState.MENU;
            inputManager.playerInput.SwitchCurrentActionMap("Menu Map");
        }
    }
    #endregion
    #region LOADING
    public void SwitchNewScene(int levelID)
    {
        switch (levelID)
        {
            case 0:
                LoadingTitle();
                break;

            case 1:
                LoadingBattle();
                break;

            case 2:
                LoadingOverworld();
                break;
        }
    }

    private void LoadTitle()
    {
        SceneManager.LoadScene(0);
    }
    private void LoadBattle()
    {
        gameManager.LastKnownScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(1);
    }
    private void LoadOverworld()
    {
        sceneChangeManager.LoadNewZone(GameManager._instance.LastKnownScene);
    }
    #endregion
}
