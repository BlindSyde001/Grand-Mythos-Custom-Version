using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteInEditMode]
public abstract class PointOfViewBase : MonoBehaviour
{
    protected const float FieldOfView = 50f;
    protected const float Ratio = 16f / 9f;
    protected const float NearPlane = 0.1f;
    protected const float FarPlane = 1f;

    protected static readonly List<PointOfViewBase> Instances = new();

    public float TransitionDuration = 0.5f;

    protected virtual void OnEnable()
    {
        Instances.Add(this);
    }

    protected virtual void OnDisable()
    {
        Instances.Remove(this);
        #if UNITY_EDITOR
        UnityEditor.SceneView.duringSceneGui -= DuringSceneGui;
        #endif
    }

    public abstract void ComputeWorldTransform(Vector3 worldPosFocus, out Vector3 position, out Quaternion rotation);

    public static PointOfViewBase FindClosest(Transform point)
    {
        var position = point.position;
        (float score, PointOfViewBase target) bestMatch = (float.PositiveInfinity, Instances.First());
        foreach (var pointOfView in Instances)
        {
            var localPos = pointOfView.transform.InverseTransformPoint(position);
            
            // We don't care about the side it is in, we only care about the distance
            var absolutePos = localPos;
            absolutePos.x = Mathf.Abs(absolutePos.x);
            absolutePos.y = Mathf.Abs(absolutePos.y);
            absolutePos.z = Mathf.Abs(absolutePos.z);

            var score = Mathf.Max(Mathf.Max(absolutePos.x, absolutePos.y), absolutePos.z);
            if (score < bestMatch.score)
                bestMatch = (score, pointOfView);
        }

        return bestMatch.target;
    }

#if UNITY_EDITOR
    private int editorSelections;

    protected virtual void DuringSceneGui(UnityEditor.SceneView obj) { }

    protected virtual void OnDrawGizmosSelected()
    {
        if (editorSelections < 0)
        {
            editorSelections = 1;
            UnityEditor.SceneView.duringSceneGui += DuringSceneGui;
        }
        else
        {
            editorSelections++;
        }

        Gizmos.color = Color.magenta;
    }

    protected virtual void OnDrawGizmos()
    {
        if (editorSelections >= 0)
        {
            editorSelections--;
            if (editorSelections == -1)
            {
                UnityEditor.SceneView.duringSceneGui -= DuringSceneGui;
            }
        }


        Gizmos.color = Color.magenta;
        var matrix = Gizmos.matrix;
        
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 2f);

        Gizmos.matrix = matrix;
    }

    [ButtonGroup]
    private void Preview()
    {
        if (UnityEditor.SceneView.lastActiveSceneView is {} view)
        {
            var viewTransform = view.camera.transform;
            ComputeWorldTransform(viewTransform.position, out var position, out var rotation);
            view.rotation = rotation;
            view.pivot = position + rotation * Vector3.forward * view.cameraDistance;
            if (Mathf.Abs(view.camera.fieldOfView - FieldOfView) > 0.1f)
                Debug.LogWarning($"Editor camera is set to a different field of view than expected ({view.camera.fieldOfView}, expected {FieldOfView})");
            viewTransform.SetPositionAndRotation(position, rotation);
        }
    }

    private static Mesh? _debugMesh;
    private static (Vector3, Vector3)[]? _ruleOfThird;

    protected static void DrawCameraFrustum()
    {
        if (_debugMesh == null && UnityEditor.SceneView.lastActiveSceneView is {} view)
        {
            var previousAspect = view.camera.aspect;
            view.camera.aspect = Ratio;
            var farCorners = new Vector3[4];
            var nearCorners = new Vector3[4];
            view.camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), FarPlane, Camera.MonoOrStereoscopicEye.Mono, farCorners);
            view.camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), NearPlane, Camera.MonoOrStereoscopicEye.Mono, nearCorners);
            view.camera.aspect = previousAspect;
            _debugMesh = new Mesh
            {
                vertices = nearCorners.Concat(farCorners).ToArray(),
                triangles = new []{ 0, 1, 5,  1, 6, 5,  1, 2, 6,  2, 7, 6,  2, 3, 7,  0, 5, 4,  3, 0, 4,  3, 4, 7, },
                normals = new []{ Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward }
            };
            _ruleOfThird = new[]
            {
                (Vector3.Lerp(farCorners[0], farCorners[1], 0.33f), Vector3.Lerp(farCorners[3], farCorners[2], 0.33f)),
                (Vector3.Lerp(farCorners[0], farCorners[1], 0.66f), Vector3.Lerp(farCorners[3], farCorners[2], 0.66f)),
                (Vector3.Lerp(farCorners[0], farCorners[3], 0.33f), Vector3.Lerp(farCorners[1], farCorners[2], 0.33f)),
                (Vector3.Lerp(farCorners[0], farCorners[3], 0.66f), Vector3.Lerp(farCorners[1], farCorners[2], 0.66f)),
            };
        }

        if (_debugMesh)
        {
            Gizmos.DrawMesh(_debugMesh);
            Gizmos.DrawFrustum(default, FieldOfView, FarPlane, NearPlane, Ratio);
            Gizmos.color = new Color(1,1,0,0.1f);
            foreach (var (start, end) in _ruleOfThird!)
            {
                Gizmos.DrawLine(start, end);
            }
        }
    }
#endif
}