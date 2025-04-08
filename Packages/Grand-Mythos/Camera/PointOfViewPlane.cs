using UnityEngine;

public class PointOfViewPlane : PointOfViewBase
{
    public Vector3 Position = new(0, 3, 0);
    public Vector3 Scale = new(1, 1, 0);
    public Quaternion Rotation = Quaternion.Euler(90, 0, 0);
    public Quaternion POVRotation = Quaternion.Euler(45, 0, 0);

    private Matrix4x4 LocalMatrix => Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
    
    public override void ComputeWorldTransform(Vector3 worldPosFocus, out Vector3 position, out Quaternion rotation)
    {
        var localFocus = Quaternion.Inverse(transform.rotation) * (worldPosFocus - transform.position);
        var plane = new Plane(Rotation * Vector3.forward, Position);
        var povDirection = POVRotation * Vector3.forward;
        plane.Raycast(new Ray(localFocus, povDirection), out float enter);
        
        localFocus += povDirection * enter; // do note that enter is more likely to be negative here, but we aren't changing it for performance purposes

        { // Clamp inside plane Scale
            // Into local plane space
            localFocus = Quaternion.Inverse(Rotation) * (localFocus - Position);
            for (int i = 0; i < 3; i++)
                localFocus[i] = Mathf.Min(Mathf.Abs(localFocus[i]), Scale[i]) * Mathf.Sign(localFocus[i]);
            // Back to local space of this transform
            localFocus = Rotation * localFocus + Position;
        }

        position = transform.rotation * localFocus + transform.position;
        rotation = transform.rotation * POVRotation;
    }
    
#if UNITY_EDITOR
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        var initialMatrix = Gizmos.matrix;
        Gizmos.matrix = LocalMatrix * Matrix4x4.TRS(Position, Rotation, Scale * 2f);
        Gizmos.DrawCube(default, Vector3.one);
        
        Gizmos.matrix = LocalMatrix * Matrix4x4.TRS(Position, POVRotation, Vector3.one);
        DrawCameraFrustum();

        Gizmos.matrix = initialMatrix;
    }

    protected override void DuringSceneGui(UnityEditor.SceneView obj)
    {
        base.DuringSceneGui(obj);
        
        var matrix = UnityEditor.Handles.matrix;
        UnityEditor.Handles.matrix = LocalMatrix;

        POVRotation = UnityEditor.Handles.RotationHandle(POVRotation, default);
        
        UnityEditor.Handles.TransformHandle(ref Position, ref Rotation, ref Scale);

        UnityEditor.Handles.matrix = matrix;
    }
#endif
}