using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Battle
{
    [AddComponentMenu(" GrandMythos/BattlePointOfView")]
    public class BattlePointOfView : MonoBehaviour
    {
        private const float FieldOfView = 60f;
        private const float Ratio = 16f / 9f;
        private const float NearPlane = 0.1f;
        public static Dictionary<BattlePointOfViewReference, BattlePointOfView> Instances = new();

        [Required]
        public BattlePointOfViewReference Reference;

        private void OnEnable()
        {
            if (Instances.TryGetValue(Reference, out var match) && match != null && match != this)
            {
                Debug.LogError($"Two {nameof(BattlePointOfView)} share the same {nameof(BattlePointOfViewReference)}, {this} and {match}", match);
                return;
            }

            Instances[Reference] = this;
        }

        private void OnDisable()
        {
            if (Instances.TryGetValue(Reference, out var pov) && pov == this)
                Instances.Remove(Reference);
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.color = Reference == null ? Color.red : new Color(1,1,0,0.05f);
            Gizmos.DrawFrustum(default, FieldOfView, 1f, NearPlane, Ratio);
        }

#if UNITY_EDITOR

        private static Mesh _debugMesh;
        private static (Vector3, Vector3)[] _ruleOfThird;
        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.color = Reference == null ? Color.red : new Color(1,1,0,1f);
            if (_debugMesh == null && UnityEditor.SceneView.lastActiveSceneView is {} view)
            {
                var previousAspect = view.camera.aspect;
                view.camera.aspect = Ratio;
                var farCorners = new Vector3[4];
                var nearCorners = new Vector3[4];
                view.camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), 100f, Camera.MonoOrStereoscopicEye.Mono, farCorners);
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
                Gizmos.DrawFrustum(default, FieldOfView, 100f, NearPlane, Ratio);
                Gizmos.color = Reference == null ? Color.red : new Color(1,1,0,0.1f);
                foreach (var (start, end) in _ruleOfThird)
                {
                    Gizmos.DrawLine(start, end);
                }
            }
        }

        [Button("Create Reference")]
        void CreateReferenceAsset()
        {
            BattlePointOfViewReference reference = ScriptableObject.CreateInstance<BattlePointOfViewReference>();
            var saveLocation = UnityEditor.EditorUtility.SaveFilePanelInProject($"Save {nameof(BattlePointOfViewReference)}", gameObject.name, "asset", $"Save {nameof(BattlePointOfViewReference)}");
            if (string.IsNullOrWhiteSpace(saveLocation))
                return;

            UnityEditor.AssetDatabase.CreateAsset(reference, saveLocation);
            Reference = reference;
        }

        [ButtonGroup]
        void MatchEditorCamera()
        {
            if (UnityEditor.SceneView.lastActiveSceneView is {} view)
            {
                var viewTransform = view.camera.transform;
                UnityEditor.Undo.RecordObject(transform, "Match Editor Camera");
                transform.SetPositionAndRotation(viewTransform.position, viewTransform.rotation);
            }
        }

        [ButtonGroup]
        void Preview()
        {
            if (UnityEditor.SceneView.lastActiveSceneView is {} view)
            {
                var viewTransform = view.camera.transform;
                view.rotation = transform.rotation;
                view.pivot = transform.position + transform.rotation * Vector3.forward * view.cameraDistance;
                if (Mathf.Abs(view.camera.fieldOfView - FieldOfView) > 0.1f)
                    Debug.LogWarning($"Editor camera is set to a different field of view than expected ({view.camera.fieldOfView}, expected {FieldOfView})");
                viewTransform.SetPositionAndRotation(transform.position, transform.rotation);
            }
        }
#endif
    }
}