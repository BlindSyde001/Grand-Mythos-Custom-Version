using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PauseHandler : MonoBehaviour
{
    public required InputActionReference[] InputsToActivate = Array.Empty<InputActionReference>();

    public required InputActionReference InputToDeactivate;

    public UnityEvent? OnPause, OnUnpause;

    void Update()
    {
        if (InputToDeactivate.action.WasPerformedThisFrameUnique())
        {
            SwitchOff();
            return;
        }

        foreach (var input in InputsToActivate)
        {
            if (input.action.WasPerformedThisFrameUnique())
            {
                SwitchOn();
                return;
            }
        }
    }

    void SwitchOn()
    {
        Time.timeScale = 0f;
        InputManager.PushGameState(GameState.Pause, this);
        OnPause?.Invoke();
    }

    void SwitchOff()
    {
        Time.timeScale = 1f;
        InputManager.PopGameState(this);
        OnUnpause?.Invoke();
    }
}