using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class OrbitCamera : ICameraControl
{
    public InputActionReference Input;
    public float Distance = 3f;
    public float MinimumAngle = -90f;
    public float MaximumAngle = 90f;
    public LayerMask ObstructionMask = 1;

    Vector3 _euler;

    public void Update(Camera camera, CameraFocus focus)
    {
        #warning lock mouse depending on the input stuff ?
        var input = Input.action.ReadValue<Vector2>();
        Input.action.Enable();
        _euler += new Vector3(input.x, -input.y, 0);
        _euler.y = Mathf.Clamp(_euler.y, MinimumAngle, MaximumAngle);

        var rotation = Quaternion.Euler(_euler.y, _euler.x, 0);
        var center = focus.transform.position;
        var direction = rotation * -Vector3.forward;

        var distance = Distance;
        if (Physics.SphereCast(center, 0.25f, direction, out var hitInfo, Distance, ObstructionMask))
            distance = hitInfo.distance;

        camera.transform.position = center + direction * distance;
        focus.transform.rotation = rotation;
        camera.transform.rotation = rotation;
    }

    public void OnValidate(CameraFocus focus)
    {
        if (Input == null)
            Debug.LogError($"{nameof(Input)} on {focus} is not set", focus);
    }

    public void OnDrawGizmos(CameraFocus focus)
    {
        Gizmos.DrawWireSphere(focus.transform.position, Distance);
    }
}