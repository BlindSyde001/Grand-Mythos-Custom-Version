using System;
using JetBrains.Annotations;
using UnityEngine;

[Serializable]
public class PointOfViewPicker : ICameraControl
{
    private PointOfViewBase? currentPointOfView;
    private (Vector3 fromP, Quaternion fromR, float time)? inTransition;
    
    public void Update(Camera camera, CameraFocus focus)
    {
        var pov = PointOfViewBase.FindClosest(focus.transform);
        if (pov != currentPointOfView)
        {
            currentPointOfView = pov;
            inTransition = (camera.transform.position, camera.transform.rotation, 0);
        }

        pov.ComputeWorldTransform(focus.transform.position, out var targetPosition, out var targetRotation);

        if (inTransition is { } transition)
        {
            transition.time += Time.deltaTime / pov.TransitionDuration;
            if (transition.time > 1)
            {
                transition.time = 1;
                inTransition = null;
            }
            else
                inTransition = transition;

            var t = Mathf.SmoothStep(0, 1, Mathf.SmoothStep(0, 1, transition.time));
            targetPosition = Vector3.Lerp(transition.fromP, targetPosition, t);
            targetRotation = Quaternion.Slerp(transition.fromR, targetRotation, t);
        }

        camera.transform.SetPositionAndRotation(targetPosition, targetRotation);
    }

    public void OnValidate(CameraFocus focus)
    {

    }

    public void OnDrawGizmos(CameraFocus focus)
    {
        
    }
}