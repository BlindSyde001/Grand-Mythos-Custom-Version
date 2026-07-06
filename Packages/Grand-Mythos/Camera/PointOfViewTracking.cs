using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class PointOfViewTracking : PointOfViewBase
{
    public Vector3 FixedPosition = Vector3.one;
    public Axis Control;
    public float LockedAxisAngle;

    public enum Axis
    {
        Unconstrained,
        HorizontalOnly,
        VerticalOnly
    }

    public override void ComputeWorldTransform(Vector3 worldPosFocus, out Vector3 position, out Quaternion rotation)
    {
        var matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one) * Matrix4x4.Translate(FixedPosition);
        position = matrix.GetPosition();
        var dir = worldPosFocus - position;
        dir = Control switch
        {
            Axis.Unconstrained => dir,
            Axis.HorizontalOnly => Quaternion.AngleAxis(LockedAxisAngle, Vector3.Cross(new Vector3(dir.x, 0, dir.z).normalized, dir.normalized)) * new Vector3(dir.x, 0, dir.z),
            Axis.VerticalOnly => Vector3.ProjectOnPlane(dir, Quaternion.AngleAxis(LockedAxisAngle, Vector3.up) * Vector3.right),
            _ => throw new ArgumentOutOfRangeException()
        };
        dir = Vector3.Normalize(dir);
        if (dir == Vector3.zero)
            dir = Vector3.Normalize(transform.position - position);
        rotation = Quaternion.LookRotation(dir, transform.up);
    }

#if UNITY_EDITOR
    protected override void DuringSceneGui(UnityEditor.SceneView obj)
    {
        var matrix = UnityEditor.Handles.matrix;
        UnityEditor.Handles.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        FixedPosition = UnityEditor.Handles.PositionHandle(FixedPosition, Quaternion.identity);
        UnityEditor.Handles.matrix = matrix;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        ComputeWorldTransform(transform.position, out var position, out var rotation);
        
        var matrix = Gizmos.matrix;
        var hMatrix = UnityEditor.Handles.matrix;
        UnityEditor.Handles.matrix = Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
        DrawCameraFrustum();

        switch (Control)
        {
            case Axis.Unconstrained:
                break;
            case Axis.HorizontalOnly:
                UnityEditor.Handles.DrawSolidDisc(Vector3.zero, Vector3.up, 0.25f);
                break;
            case Axis.VerticalOnly:
                UnityEditor.Handles.DrawSolidDisc(Vector3.zero, Vector3.right, 0.25f);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        UnityEditor.Handles.matrix = hMatrix;
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
        }
    }
#endif
}