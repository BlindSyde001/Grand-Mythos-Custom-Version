using System;
using UnityEngine;

[Serializable]
public class FixedCamera : ICameraControl
{
    public void Update(Camera camera, CameraFocus focus)
    {
        camera.transform.position = focus.transform.position;
        camera.transform.rotation = focus.transform.rotation;
    }

    public void OnValidate(CameraFocus focus)
    {

    }

    public void OnDrawGizmos(CameraFocus focus)
    {
        Gizmos.matrix = Matrix4x4.TRS(focus.transform.position, focus.transform.rotation, Vector3.one);
        Gizmos.DrawFrustum(center:default, fov:60, maxRange:0.25f, minRange:0.1f, aspect:1.777f);
    }
}