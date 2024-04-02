using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFocusSwitch : ICameraControl
{
    [SerializeReference, Required] public ICameraControl A;
    [SerializeReference, Required] public ICameraControl B;
    [Required] public InputActionReference SwitchInput;
    public float TransitionDuration = 0.5f;

    float _t;
    float _dir;

    public void Update(Camera camera, CameraFocus focus)
    {
        if (SwitchInput.action.WasReleasedThisFrame())
        {
            if (_dir == 0)
                _dir = _t == 1f ? -1 : 1;
            else
                _dir = -_dir;
        }

        if (_dir != 0)
        {
            _t += _dir * Time.deltaTime / TransitionDuration;
            _t = Mathf.Clamp01(_t);
            if (_t is 0f or 1f)
                _dir = 0f;
        }

        if (_t == 1f)
        {
            B.Update(camera, focus);
        }
        else if (_t == 0f)
        {
            A.Update(camera, focus);
        }
        else
        {
            var smoothT = Mathf.SmoothStep(0, 1f, Mathf.SmoothStep(0, 1f, _t));

            A.Update(camera, focus);
            var p = camera.transform.position;
            var r = camera.transform.rotation;
            B.Update(camera, focus);
            p = Vector3.Lerp(p, camera.transform.position, smoothT);
            r = Quaternion.Slerp(r, camera.transform.rotation, smoothT);
            camera.transform.SetPositionAndRotation(p, r);
        }
    }

    public void OnValidate(CameraFocus focus)
    {
        A?.OnValidate(focus);
        B?.OnValidate(focus);
    }

    public void OnDrawGizmos(CameraFocus focus)
    {
        A?.OnDrawGizmos(focus);
        B?.OnDrawGizmos(focus);
    }
}