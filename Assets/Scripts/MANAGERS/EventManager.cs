using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState {BATTLE, OVERWORLD, TITLE, CUTSCENE, MENU }
public enum CombatState {WAIT, ACTIVE}
public class EventManager : MonoBehaviour
{
    // VARIABLES
    public static EventManager _instance;

    [SerializeField]
    internal GameState _GameState;

    public delegate void LoadNewScene();
    public static event LoadNewScene LoadingOverworld;
    public static event LoadNewScene LoadingBattle;
    public static event LoadNewScene LoadingTitle;

    public delegate void ChangeInGameState(GameState GS);
    public static event ChangeInGameState ChangeToBattleState;
    public static event ChangeInGameState ChangeToOverworldState;
    public static event ChangeInGameState ChangeToTitleState;
    public static event ChangeInGameState ChangeToCutsceneState;

    public delegate void ChangeZone();
    public static event ChangeZone OnZoneChanged;

    public delegate void DataManipulation();
    public static event DataManipulation SaveTheGame;
    public static event DataManipulation LoadTheGame;


    // UPDATES
    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
        }
        else if(_instance != this)
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        LoadingOverworld += LoadOverworld;
        LoadingBattle += LoadBattle;
        LoadingTitle += LoadTitle;

        ChangeToBattleState += BattleState;
        ChangeToOverworldState += OverworldState;
        ChangeToTitleState += TitleState;
        ChangeToCutsceneState += CutsceneState;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        LoadingOverworld -= LoadOverworld;
        LoadingBattle -= LoadBattle;
        LoadingTitle -= LoadTitle;

        ChangeToBattleState -= BattleState;
        ChangeToOverworldState -= OverworldState;
        ChangeToTitleState -= TitleState;
        ChangeToCutsceneState -= CutsceneState;
    }

    // METHODS
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("GameState Changed");
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
        GameManager._instance._LastKnownScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(1);
    }
    private void LoadOverworld()
    {
        SceneChangeManager._instance.LoadNewZone(GameManager._instance._LastKnownScene);
    }
    #endregion
    #region GAMESTATE CHANGING
    public void SwitchGameState(GameState GS)
    {
        switch (GS)
        {
            case GameState.TITLE:
                GS = GameState.TITLE;
                ChangeToTitleState(GS);
                break;

            case GameState.OVERWORLD:
                GS = GameState.OVERWORLD;
                ChangeToOverworldState(GS);
                break;

            case GameState.BATTLE:
                GS = GameState.BATTLE;
                ChangeToBattleState(GS);
                break;

            case GameState.CUTSCENE:
                GS = GameState.CUTSCENE;
                ChangeToCutsceneState(GS);
                break;

            case GameState.MENU:
                GS = GameState.MENU;
                MenuState(GS);
                break;
        }
    }

    private void OverworldState(GameState GS)
    {
        _GameState = GameState.OVERWORLD;
        InputManager._instance.playerInput.SwitchCurrentActionMap("Overworld Map");
    }
    private void BattleState(GameState GS)
    {
        _GameState = GameState.BATTLE;
        InputManager._instance.playerInput.SwitchCurrentActionMap("Battle Map");
    }
    private void TitleState(GameState GS)
    {
        _GameState = GameState.TITLE;
        InputManager._instance.playerInput.SwitchCurrentActionMap("Title Map");
    }
    private void CutsceneState(GameState GS)
    {
        _GameState = GameState.CUTSCENE;
        InputManager._instance.playerInput.SwitchCurrentActionMap("Cutscene Map");
    }
    private void MenuState(GameState GS)
    {
        _GameState = GameState.MENU;
        InputManager._instance.playerInput.SwitchCurrentActionMap("Menu Map");
    }
    #endregion
}
