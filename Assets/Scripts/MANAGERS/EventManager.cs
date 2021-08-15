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
    
    // UPDATES
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

    }
    private void OnEnable()
    {
        ChangeToBattleState += BattleLoad;
    }
    private void OnDisable()
    {
        ChangeToBattleState -= BattleLoad;
    }

    // METHODS
    public void ChangeFunction(GameState GS)
    {
        switch (GS)
        {
            case GameState.TITLE:
                GS = GameState.TITLE;

                break;

            case GameState.OVERWORLD:
                GS = GameState.OVERWORLD;

                break;

            case GameState.BATTLE:
                GS = GameState.BATTLE;
                ChangeToBattleState(GS);
                break;

            case GameState.CUTSCENE:
                GS = GameState.CUTSCENE;

                break;
        }
    }
    private void BattleLoad(GameState GS)
    {
        GM._LastKnownScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("TEST - Battle");
    }
}
