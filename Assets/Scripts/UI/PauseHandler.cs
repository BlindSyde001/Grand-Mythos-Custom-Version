using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PauseHandler : MonoBehaviour
{
    [Required]
    public InputActionReference[] InputsToActivate = Array.Empty<InputActionReference>();

    [Required]
    public InputActionReference InputToDeactivate;

    public UnityEvent OnPause, OnUnpause;

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
        OnPause.Invoke();
    }

    void SwitchOff()
    {
        Time.timeScale = 1f;
        InputManager.PopGameState(this);
        OnUnpause.Invoke();
    }
}