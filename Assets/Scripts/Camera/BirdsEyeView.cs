using System;
using UnityEngine;

[Serializable]
public class BirdsEyeView : ICameraControl
{
    public float Distance = 10f;
    public Quaternion WorldRotation = Quaternion.identity;

    public void Update(Camera camera, CameraFocus focus)
    {
        camera.transform.SetPositionAndRotation(focus.transform.position + WorldRotation * Vector3.back * Distance, WorldRotation);
    }

    public void OnValidate(CameraFocus focus)
    {

    }

    public void OnDrawGizmos(CameraFocus focus)
    {
        Gizmos.matrix = Matrix4x4.TRS(focus.transform.position + WorldRotation * Vector3.back * Distance, WorldRotation, Vector3.one);
        Gizmos.DrawFrustum(center:default, fov:60, maxRange:0.25f, minRange:0.1f, aspect:1.777f);
    }
}