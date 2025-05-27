using UnityEngine;

public class PointOfViewRelative : PointOfViewBase
{
    public Vector3 Offset = new Vector3(0, 0, 5f);
    public Quaternion Rotation = Quaternion.Euler(45, 0, 0);

    private Matrix4x4 LocalMatrix => Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
    
    public override void ComputeWorldTransform(Vector3 worldPosFocus, out Vector3 position, out Quaternion rotation)
    {
        rotation = transform.rotation * Rotation;
        position = worldPosFocus + rotation * -Offset;
    }
    
#if UNITY_EDITOR
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        var initialMatrix = Gizmos.matrix;
        
        Gizmos.matrix = LocalMatrix;
        Gizmos.DrawWireSphere(Vector3.zero, 0.1f);
        Gizmos.DrawLine(Vector3.zero, Rotation * -Offset);
        Gizmos.matrix = LocalMatrix * Matrix4x4.Rotate(Rotation) * Matrix4x4.Translate(-Offset);
        DrawCameraFrustum();

        Gizmos.matrix = initialMatrix;
    }

    protected override void DuringSceneGui(UnityEditor.SceneView obj)
    {
        base.DuringSceneGui(obj);
        
        var matrix = UnityEditor.Handles.matrix;
        UnityEditor.Handles.matrix = LocalMatrix;

        Rotation = UnityEditor.Handles.RotationHandle(Rotation, default);

        if (Event.current.type == EventType.Repaint)
        {
            var center = Rotation * -Offset;
            UnityEditor.Handles.ArrowHandleCap(0, center, Quaternion.identity, 0.5f, EventType.Repaint);
            UnityEditor.Handles.ArrowHandleCap(0, center, Quaternion.Euler(0, 180, 0), 0.5f, EventType.Repaint);
            UnityEditor.Handles.ArrowHandleCap(0, center, Quaternion.Euler(+90, 0, 0), 0.5f, EventType.Repaint);
            UnityEditor.Handles.ArrowHandleCap(0, center, Quaternion.Euler(-90, 0, 0), 0.5f, EventType.Repaint);
            UnityEditor.Handles.ArrowHandleCap(0, center, Quaternion.Euler(0, +90, 0), 0.5f, EventType.Repaint);
            UnityEditor.Handles.ArrowHandleCap(0, center, Quaternion.Euler(0, -90, 0), 0.5f, EventType.Repaint);
        }

        UnityEditor.Handles.matrix = matrix;
    }
#endif
}