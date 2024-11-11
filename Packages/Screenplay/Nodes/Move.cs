using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Screenplay.Nodes
{
    public class Move : Action, INodeWithSceneGizmos
    {
        [HideLabel] public SceneObjectReference<GameObject> Target;
        [HideLabel] public Vector3 Destination;
        [HideLabel] public Quaternion Rotation = Quaternion.identity;

        public override void CollectReferences(List<GenericSceneObjectReference> references) => references.Add(Target);

        public override IEnumerable<Signal> Execute(IContext context)
        {
            if (Target.TryGet(out var go, out var failure) == false)
            {
                Debug.LogWarning($"Failed to {nameof(Move)}, {nameof(Target)}: {failure}", context.Source);
                yield return Signal.BreakInto(Next);
                yield break;
            }

            go.transform.position = Destination;
            go.transform.rotation = Rotation;
            yield return Signal.BreakInto(Next);
        }

        public override void FastForward(IContext context)
        {
            if (Target.TryGet(out var go, out var failure) == false)
            {
                Debug.LogWarning($"Failed to {nameof(Move)}, {nameof(Target)}: {failure}", context.Source);
                return;
            }

            go.transform.position = Destination;
            go.transform.rotation = Rotation;
        }

        public override void SetupPreview(IPreviewer previewer, bool fastForwarded)
        {
            if (Target.TryGet(out var go, out var failure) == false)
                return;

            var previousPosition = go.transform.position;
            var previousRotation = go.transform.rotation;
            previewer.RegisterRollback(() =>
            {
                go.transform.position = previousPosition;
                go.transform.rotation = previousRotation;
            });

            go.transform.position = Destination;
            go.transform.rotation = Rotation;
        }

        public void DrawGizmos(ref bool rebuildPreview)
        {
            #if UNITY_EDITOR
            var newPosition = UnityEditor.Handles.PositionHandle(Destination, Rotation);
            var newRotation = UnityEditor.Handles.RotationHandle(Rotation, Destination);
            rebuildPreview |= newPosition != Destination || newRotation != Rotation;
            Destination = newPosition;
            Rotation = newRotation;
            #endif
        }
    }
}
