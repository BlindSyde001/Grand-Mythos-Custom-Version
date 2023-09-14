using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState {BATTLE, OVERWORLD, TITLE, CUTSCENE, MENU }
public enum CombatState {START, ACTIVE, WAIT, END}
public class EventManager : MonoBehaviour
{
    // VARIABLES
    public static EventManager _instance;

    [SerializeField]
    private InputManager inputManager;

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
            return;
        }
        this.transform.parent = null;
        DontDestroyOnLoad(gameObject);
    }
    private void OnEnable()
    {
        Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;

        OnGameStateChange += TitleState;
        OnGameStateChange += OverworldState;
        OnGameStateChange += BattleState;
        OnGameStateChange += CutsceneState;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

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
        //Debug.Log(GS);
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
            inputManager.playerInput.SwitchCurrentActionMap("Title Map");
        }
    }
    private void OverworldState(GameState GS)
    {
        if(GS == GameState.OVERWORLD)
        {
            inputManager.playerInput.SwitchCurrentActionMap("Overworld Map");
            foreach (var instance in OverworldPlayerControlsNode.Instances)
                instance.enabled = true;
        }
    }
    private void BattleState(GameState GS)
    {
        if(GS == GameState.BATTLE)
        {
            inputManager.playerInput.SwitchCurrentActionMap("Battle Map");
        }
    }
    private void CutsceneState(GameState GS)
    {
        if(GS == GameState.CUTSCENE)
        {
            inputManager.playerInput.SwitchCurrentActionMap("Cutscene Map");
            foreach (var instance in OverworldPlayerControlsNode.Instances)
                instance.enabled = false;
        }
    }
    private void MenuState(GameState GS)
    {
        if(GS == GameState.MENU)
        {
            inputManager.playerInput.SwitchCurrentActionMap("Menu Map");
        }
    }
    #endregion
}
