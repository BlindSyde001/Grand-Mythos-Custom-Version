using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.UI;
using Object = UnityEngine.Object;

public static class InputManager
{
    public static GameState CurrentState { get; private set; } = GameState.Menu;
    public static List<GameStateRequests> StateStack = new();
    static InputActionAsset PlayerInput => SingletonManager.Instance.PlayerInput;
    static InputSystemUIInputModule Module;

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

        if (ReferenceEquals(Module, null))
            throw new NullReferenceException(nameof(Module));
        else if (Module != null) // Exist but not destroyed, if it doesn't exist it better throw which is why we have the exception above
        {
            Module.move = InputActionReference.Create(currentMap.actions.FirstOrDefault(x => x.name == "Navigate"));
            Module.submit = InputActionReference.Create(currentMap.actions.FirstOrDefault(x => x.name == "Submit"));
            Module.cancel = InputActionReference.Create(currentMap.actions.FirstOrDefault(x => x.name == "Cancel"));
            Module.point = InputActionReference.Create(currentMap.actions.FirstOrDefault(x => x.name == "Point"));
            Module.leftClick = InputActionReference.Create(currentMap.actions.FirstOrDefault(x => x.name == "Click"));
            Module.middleClick = InputActionReference.Create(currentMap.actions.FirstOrDefault(x => x.name == "MiddleClick"));
            Module.rightClick = InputActionReference.Create(currentMap.actions.FirstOrDefault(x => x.name == "RightClick"));
            Module.scrollWheel = InputActionReference.Create(currentMap.actions.FirstOrDefault(x => x.name == "ScrollWheel"));
            Module.trackedDevicePosition = InputActionReference.Create(currentMap.actions.FirstOrDefault(x => x.name == "TrackedDevicePosition"));
            Module.trackedDeviceOrientation = InputActionReference.Create(currentMap.actions.FirstOrDefault(x => x.name == "TrackedDeviceOrientation"));
        }

        CurrentState = newState;
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
        var eventSystem = SingletonManager.Instance.DefaultEventSystem.gameObject;
        var go = Object.Instantiate(eventSystem);
        Module = go.GetComponent<InputSystemUIInputModule>();
        go.AddComponent<InputManagement>();
        Object.DontDestroyOnLoad(go);
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

    public class InputManagement : MonoBehaviour
    {
        bool _inDesktopMode = true;

        void Update()
        {
            (CursorLockMode lockMode, bool visible) targetState = CurrentState switch
            {
                GameState.Battle => (CursorLockMode.None, true),
                GameState.Overworld => (CursorLockMode.Locked, false),
                GameState.Cutscene => (CursorLockMode.None, true),
                GameState.Menu => (CursorLockMode.None, true),
                GameState.Pause => (CursorLockMode.None, true),
                _ => throw new ArgumentOutOfRangeException()
            };

            if (_inDesktopMode && Gamepad.current != null)
            {
                foreach (var ctrl in Gamepad.current.allControls)
                {
                    if (ctrl is ButtonControl { isPressed: true, synthetic: false })
                    {
                        _inDesktopMode = false;
                        break;
                    }
                }
            }
            else
            {
                if (Keyboard.current != null)
                {
                    foreach (var ctrl in Keyboard.current.allControls)
                    {
                        if (ctrl is ButtonControl { isPressed: true, synthetic: false })
                        {
                            _inDesktopMode = true;
                            break;
                        }
                    }
                }

                if (Mouse.current != null)
                {
                    foreach (var ctrl in Mouse.current.allControls)
                    {
                        if (ctrl is ButtonControl { isPressed: true, synthetic: false })
                        {
                            _inDesktopMode = true;
                            break;
                        }
                    }
                }
            }

            if (_inDesktopMode)
            {
                Cursor.visible = targetState.visible;
                Cursor.lockState = targetState.lockMode;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Confined;
            }
        }
    }
}

public partial class DomainReloadHelper
{
    public List<InputManager.GameStateRequests> InputManagerInstance;
}