using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[Serializable]
public class OrbitCamera : ICameraControl
{
    public required InputActionReference Input, Zoom;
    public float DistanceDefault = 3f;
    [FormerlySerializedAs("Distance")] public float DistanceMinimum = 3f;
    public float DistanceMaximum = 10f;
    public float MinimumAngle = -90f;
    public float MaximumAngle = 90f;
    public LayerMask ObstructionMask = 1;

    Vector3 _euler;
    bool _initialized;

    public void Update(Camera camera, CameraFocus focus)
    {
        if (_initialized == false)
        {
            _initialized = true;
            _euler = focus.transform.eulerAngles;
            (_euler.x, _euler.y) = (_euler.y, _euler.x);
            _euler.z = 0;
        }

        var input = Input.action.ReadValue<Vector2>();
        _euler += new Vector3(input.x, -input.y, 0);
        _euler.y = Mathf.Clamp(_euler.y, MinimumAngle, MaximumAngle);

        DistanceDefault += Zoom.action.ReadValue<float>() * Time.deltaTime;
        DistanceDefault = Mathf.Clamp(DistanceDefault, DistanceMinimum, DistanceMaximum);

        var rotation = Quaternion.Euler(_euler.y, _euler.x, 0);
        var center = focus.transform.position;
        var direction = rotation * -Vector3.forward;

        var distance = DistanceDefault;
        if (Physics.SphereCast(center, 0.25f, direction, out var hitInfo, distance, ObstructionMask))
            distance = hitInfo.distance;

        camera.transform.SetPositionAndRotation(center + direction * distance, rotation);
    }

    public void OnValidate(CameraFocus focus)
    {
        if (Input == null!)
            Debug.LogError($"{nameof(Input)} on {focus} is not set", focus);
    }

    public void OnDrawGizmos(CameraFocus focus)
    {
        Gizmos.DrawWireSphere(focus.transform.position, DistanceMinimum);
    }
}