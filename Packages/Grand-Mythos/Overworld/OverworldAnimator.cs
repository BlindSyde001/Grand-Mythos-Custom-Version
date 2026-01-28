using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu(" GrandMythos/OverworldAnimator")]
public class OverworldAnimator : MonoBehaviour
{
    public required OverworldPlayerController Controller;
    public UnityEvent? OnStartMove;
    public UnityEvent? OnStartIdle;

    Vector3 _prevPosition;
    bool _wasMoving;

    void Update()
    {
        bool isMoving = _prevPosition != Controller.transform.position;
        _prevPosition = Controller.transform.position;

        if (_wasMoving != isMoving)
        {
            (isMoving ? OnStartMove : OnStartIdle)?.Invoke();
            _wasMoving = isMoving;
        }
    }
}