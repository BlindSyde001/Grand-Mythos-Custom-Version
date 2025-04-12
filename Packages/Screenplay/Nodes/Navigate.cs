using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using YNode;

namespace Screenplay.Nodes
{
    [Serializable, NodeWidth(NodeWidthAttribute.Default * 2)]
    public class Navigate : Action, INodeWithSceneGizmos
    {
        [HideLabel] public SceneObjectReference<NavMeshAgent> Target;
        public Vector3 Destination;
        public Quaternion Rotation = Quaternion.identity;
        public float RotationDuration = 0.5f;

        public override void CollectReferences(List<GenericSceneObjectReference> references) => references.Add(Target);

        public override IEnumerable<Signal> Execute(IContext context)
        {
            if (Target.TryGet(out var agent, out var failure) == false)
            {
                Debug.LogWarning($"Failed to {nameof(Navigate)}, {nameof(Target)}: {failure}", context.Source);
                yield return Signal.BreakInto(Next);
                yield break;
            }

            agent.SetDestination(Destination);
            do
            {
                yield return Signal.NextFrame;
            } while (agent.pathPending);

            if (agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                do
                {
                    yield return Signal.NextFrame;
                } while (agent.remainingDistance >= float.Epsilon);
            }

            var previousPos = agent.transform.position;
            var previousRot = agent.transform.rotation;
            float t = 0;
            do
            {
                t += Time.deltaTime / RotationDuration;
                float smoothT = Mathf.SmoothStep(0, 1, Mathf.SmoothStep(0, 1, t));
                agent.transform.position = Vector3.Lerp(previousPos, Destination, smoothT);
                agent.transform.rotation = Quaternion.Slerp(previousRot, Rotation, smoothT);
                yield return Signal.NextFrame;
            } while (t < 1f);

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
            if (Target.TryGet(out var go, out var failure) && NavMesh.SamplePosition(Destination, out NavMeshHit hit, 10f, go.areaMask))
                Destination = hit.position;
#endif
        }

    }
}
