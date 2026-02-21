using System.Threading;
using Characters;
using Cysharp.Threading.Tasks;
using Screenplay;
using Screenplay.Nodes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using YNode;

namespace Quests
{
    [NodeVisuals(Icon = "d_NavMeshAgent Icon")]
    public class NPCMoveTo : ExecutableLinear, INodeWithSceneGizmos
    {
        [HideLabel] public SceneObjectReference<NPC> NPC;
        public Vector3 Destination;
        public Quaternion FinalRotation = Quaternion.identity;
        
        public override void CollectReferences(ReferenceCollector references) => references.Collect(NPC);

        protected override async UniTask LinearExecution(IEventContext context, CancellationToken cancellation)
        {
            var npc = await NPC.GetAsync(cancellation);
            await npc.MoveTo(Destination, FinalRotation, cancellation);
        }

        public override async UniTask Persistence(IEventContext context, CancellationToken cancellationToken)
        {
            var npc = await NPC.GetAsync(cancellationToken);
            npc.transform.position = Destination;
            npc.transform.rotation = FinalRotation;
        }

        public override void SetupPreview(IPreviewer previewer, bool fastForwarded)
        {
            if (NPC.TryGet(out var npc, out _))
            {
                previewer.RegisterTRSRollback(npc.transform);

                npc.transform.position = Destination;
                npc.transform.rotation = FinalRotation;
            }
        }

        public void DrawGizmos(SceneGUIProxy guiProxy, ScreenplayGraph graph, ref bool rebuildPreview)
        {
            using var _ = guiProxy.TempChanges();
            if (NPC.TryGet(out var go, out var failure))
            {
                if (NavMesh.SamplePosition(Destination, out NavMeshHit hit, 1f, go.Agent.areaMask))
                {
                    if (Destination != hit.position)
                        guiProxy.SetDirty(graph);

                    Destination = hit.position;
                }
                else
                {
                    guiProxy.Color(Color.red);
                    guiProxy.Label(Destination, "No navmeshes");
                }

                var path = new NavMeshPath();
                if (NavMesh.CalculatePath(go.transform.position, Destination, go.Agent.areaMask, path))
                {
                    var previous = go.transform.position;
                    foreach (var pathCorner in path.corners)
                    {
                        guiProxy.DottedLine(previous, pathCorner);
                        previous = pathCorner;
                    }
                }
                else
                {
                    guiProxy.Color(Color.red);
                    guiProxy.Label(Destination, "No path");
                    guiProxy.DottedLine(go.transform.position, Destination);
                }
            }

            Destination = guiProxy.PositionHandle(Destination, FinalRotation);
            FinalRotation = guiProxy.RotationHandle(FinalRotation, Destination);
            guiProxy.Arrow(Destination + Vector3.up * 0.5f, FinalRotation, 1f);
        }
    }
}
