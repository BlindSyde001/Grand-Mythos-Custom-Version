using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

public class PointOfViewFixed : PointOfViewBase
{
    public Vector3 FixedPosition = Vector3.one;
    public Quaternion FixedRotation = Quaternion.Euler(45f, 45f, 45f);

    public override void ComputeWorldTransform(Vector3 worldPosFocus, out Vector3 position, out Quaternion rotation)
    {
        var matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one) * Matrix4x4.TRS(FixedPosition, FixedRotation, Vector3.one);
        rotation = matrix.rotation;
        position = matrix.GetPosition();
    }

#if UNITY_EDITOR
    protected override void DuringSceneGui(UnityEditor.SceneView obj)
    {
        var matrix = UnityEditor.Handles.matrix;
        UnityEditor.Handles.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        FixedPosition = UnityEditor.Handles.PositionHandle(FixedPosition, FixedRotation);
        FixedRotation = UnityEditor.Handles.RotationHandle(FixedRotation, FixedPosition);
        UnityEditor.Handles.matrix = matrix;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        ComputeWorldTransform(default, out var position, out var rotation);
        
        var matrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
        DrawCameraFrustum();
        Gizmos.matrix = matrix;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
    }


    [ButtonGroup]
    void MatchEditorCamera()
    {
        if (UnityEditor.SceneView.lastActiveSceneView is {} view)
        {
            var viewTransform = view.camera.transform;
            UnityEditor.Undo.RecordObject(transform, "Match Editor Camera");

            var invQ = Quaternion.Inverse(transform.rotation);
            FixedPosition = invQ * (viewTransform.position - transform.position);
            FixedRotation = invQ * viewTransform.rotation;
        }
    }
#endif
}
