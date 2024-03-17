using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

public class InputManager : MonoBehaviour
{
    // VARIABLES
    public static InputManager Instance { get; private set; }

    public PlayerInput PlayerInput;

    public GameState CurrentState { get; private set; }
    public List<GameStateRequests> StateStack = new();

    // UPDATES
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        SetGameState(GameState.Menu);
        this.transform.parent = null;
        DontDestroyOnLoad(this.gameObject);
    }

    // METHODS
    #region INPUT COMMANDS


    /// <summary>
    /// Push a new gamestate to change the control scheme, the key is used when poping said game state.
    /// </summary>
    /// <remarks>
    /// When poping a gamestate, the last gamestate that was pushed before that one is used as the new gamestate.
    /// </remarks>
    public void PushGameState(GameState newState, Object key)
    {
        StateStack.Add(new(){ State = newState, Key = key });

        if (PlayerInput == null) // When unloading the game input gets destroyed but this function will still be called
            return;

        SetGameState(newState);
    }

    /// <summary>
    /// Pop the gamestate associated with this key, setting the gamestate to the last one set before that one.
    /// </summary>
    public void PopGameState(Object key)
    {
        for (int i = StateStack.Count - 1; i >= 0; i--)
        {
            if (StateStack[i].Key == null)
                StateStack.RemoveAt(i);
        }

        for (int i = StateStack.Count - 1; i >= 0; i--)
        {
            if (StateStack[i].Key == key)
            {
                StateStack.RemoveAt(i);
                break;
            }
        }

        if (PlayerInput == null) // When unloading the game input gets destroyed but this function will still be called
            return;

        if (StateStack.Count > 0 && StateStack[^1].State != CurrentState)
            SetGameState(StateStack[^1].State);
        else
            SetGameState(GameState.Overworld);
    }

    void SetGameState(GameState newState)
    {
        CurrentState = newState;
        #warning Will have to rethink this OverworldPlayerController thing
        switch (newState)
        {
            case GameState.Overworld:
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                /*foreach (var instance in OverworldPlayerController.Instances)
                    instance.enabled = true;*/
                break;

            case GameState.Cutscene:
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                /*foreach (var instance in OverworldPlayerController.Instances)
                    instance.enabled = false;*/
                break;

            default:
            case GameState.Battle:
            case GameState.Menu:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
        }

        PlayerInput.SwitchCurrentActionMap(newState switch
        {
            GameState.Overworld => "Overworld Map",
            GameState.Battle => "Battle Map",
            GameState.Cutscene => "Cutscene Map",
            GameState.Menu => "Menu Map",
            _ => throw new ArgumentOutOfRangeException(nameof(newState), newState, null)
        });
    }
    #endregion

    [Serializable]
    public struct GameStateRequests
    {
        public GameState State;
        public Object Key;
    }

    static InputManager()
    {
        DomainReloadHelper.BeforeReload += helper => helper.InputManagerInstance = Instance;
        DomainReloadHelper.AfterReload += helper => Instance = helper.InputManagerInstance;
    }
}

public partial class DomainReloadHelper
{
    public InputManager InputManagerInstance;
}