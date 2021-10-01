using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState {BATTLE, OVERWORLD, TITLE, CUTSCENE }
public enum BattleState {WAIT, ACTIVE}
public class EventManager : MonoBehaviour
{
    // VARIABLES
    [SerializeField]
    internal GameManager GM;
    [SerializeField]
    internal GameState _GameState;

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

    private static EventManager _instance;
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
        ChangeToBattleState += BattleLoad;
        ChangeToOverworldState += OverworldLoad;
        ChangeToTitleState += TitleLoad;
        ChangeToCutsceneState += CutsceneLoad;
    }
    private void OnDisable()
    {
        ChangeToBattleState -= BattleLoad;
        ChangeToOverworldState -= OverworldLoad;
        ChangeToTitleState -= TitleLoad;
        ChangeToCutsceneState -= CutsceneLoad;
    }

    // METHODS
    public void ChangeFunction(GameState GS)
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
        }
    }

    private void OverworldLoad(GameState GS)
    {
        SceneManager.LoadScene(GM._LastKnownScene);
    }
    private void BattleLoad(GameState GS)
    {
        GM._LastKnownScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("TEST - Battle");
    }
    private void TitleLoad(GameState GS)
    {
        SceneManager.LoadScene("TEST - Title");
    }
    private void CutsceneLoad(GameState GS)
    {

    }
}
