using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ToggleWithInput : MonoBehaviour
{
    public required InputActionReference Input;
    public bool State;
    public UnityEvent? WhenOn, WhenOff;

    void Update()
    {
        if (Input.action.WasPerformedThisFrameUnique())
        {
            State = !State;
            if (State)
                WhenOn?.Invoke();
            else
                WhenOff?.Invoke();
        }
    }
}