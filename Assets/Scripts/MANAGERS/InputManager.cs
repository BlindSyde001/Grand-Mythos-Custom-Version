using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

public static class InputManager
{
    public static GameState CurrentState { get; private set; } = GameState.Menu;
    public static List<GameStateRequests> StateStack = new();
    static InputActionAsset PlayerInput => SingletonManager.Instance.PlayerInput;

    /// <summary>
    /// Push a new gamestate to change the control scheme, the key is used when poping said game state.
    /// </summary>
    /// <remarks>
    /// When poping a gamestate, the last gamestate that was pushed before that one is used as the new gamestate.
    /// </remarks>
    public static void PushGameState(GameState newState, Object key)
    {
        StateStack.Add(new(){ State = newState, Key = key });

        if (PlayerInput == null) // When unloading the game input gets destroyed but this function will still be called
            return;

        SetGameState(newState);
    }

    /// <summary>
    /// Pop the gamestate associated with this key, setting the gamestate to the last one set before that one.
    /// </summary>
    public static void PopGameState(Object key)
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

        if (PlayerInput == null && ReferenceEquals(PlayerInput, null) == false) // When unloading the game input gets destroyed but this function will still be called
            return;

        if (StateStack.Count > 0 && StateStack[^1].State != CurrentState)
            SetGameState(StateStack[^1].State);
        else
            SetGameState(GameState.Menu);
    }

    static void SetGameState(GameState newState)
    {
        var oldMap = PlayerInput.FindActionMap(StateToMap(CurrentState));
        var currentMap = PlayerInput.FindActionMap(StateToMap(newState));
        oldMap.Disable();
        currentMap.Enable();
        foreach (var action in oldMap.actions)
            action.Disable();
        foreach (var action in currentMap.actions)
            action.Enable();

        CurrentState = newState;
        switch (newState)
        {
            case GameState.Overworld:
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                break;

            case GameState.Cutscene:
            case GameState.Battle:
            case GameState.Menu:
            case GameState.Pause:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
        }
    }

    static string StateToMap(GameState state)
    {
        return state switch
        {
            GameState.Overworld => "Overworld Map",
            GameState.Battle => "Battle Map",
            GameState.Cutscene => "Cutscene Map",
            GameState.Menu => "Menu Map",
            GameState.Pause => "Pause Map",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitInputManager()
    {
        SetGameState(GameState.Menu);
    }

    [Serializable]
    public struct GameStateRequests
    {
        public GameState State;
        public Object Key;
    }

    static InputManager()
    {
        DomainReloadHelper.BeforeReload += helper => helper.InputManagerInstance = StateStack;
        DomainReloadHelper.AfterReload += helper => StateStack = helper.InputManagerInstance;
    }
}

public partial class DomainReloadHelper
{
    public List<InputManager.GameStateRequests> InputManagerInstance;
}