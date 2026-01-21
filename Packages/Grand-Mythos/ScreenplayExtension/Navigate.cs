using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Screenplay;
using Screenplay.Nodes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using YNode;

[Serializable, NodeVisuals(Width = NodeVisualsAttribute.DefaultWidth * 2)]
public class Navigate : ExecutableLinear, INodeWithSceneGizmos
{
    [HideLabel] public SceneObjectReference<NavMeshAgent> Target;
    public Vector3 Destination;
    public Quaternion Rotation = Quaternion.identity;
    public float RotationDuration = 0.5f;

    public override void CollectReferences(ReferenceCollector references) => references.Collect(Target);

    protected override async UniTask LinearExecution(IEventContext context, CancellationToken cancellation)
    {
        if (Target.TryGet(out var agent, out var failure) == false)
        {
            Debug.LogWarning($"Failed to {nameof(Navigate)}, {nameof(Target)}: {failure}", context.Source);
            return;
        }

        agent.SetDestination(Destination);
        do
        {
            await UniTask.NextFrame(cancellation, cancelImmediately: true);
        } while (agent.pathPending);

        if (agent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            do
            {
                await UniTask.NextFrame(cancellation, cancelImmediately: true);
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
            await UniTask.NextFrame(cancellation, cancelImmediately: true);
        } while (t < 1f);
    }

    public override void FastForward(IEventContext context, CancellationToken cancellationToken)
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