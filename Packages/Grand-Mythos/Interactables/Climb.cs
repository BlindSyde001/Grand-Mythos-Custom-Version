using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace Interactables
{
    public class Climb : IInteraction
    {
        public float Duration = 1f;
        public Vector3 P1 = new(0, 1, 0), P2 = new(0, -1, 0);
        public Quaternion Rotation = Quaternion.identity;

        private Matrix4x4 LocalMatrix(Transform transform) => Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        
        public async UniTask InteractEnum(IInteractionSource source, OverworldPlayerController player)
        {
            var pPos = player.transform.position;
            var pRot = player.transform.rotation;

            var P1World = source.transform.position + source.transform.rotation * P1;
            var P2World = source.transform.position + source.transform.rotation * P2;
            var QWorld = source.transform.rotation * Rotation;
            
            var (start, end) = Vector3.Distance(pPos, P1World) > Vector3.Distance(pPos, P2World) ? (P2World, P1World) : (P1World, P2World);
            
            for (float i = Time.deltaTime; i <= 1f; i += Time.deltaTime * 5f)
            {
                player.transform.position = Vector3.Lerp(pPos, start, i);
                player.transform.rotation = Quaternion.Lerp(pRot, QWorld, i);
                await UniTask.NextFrame();
            }

            player.transform.rotation = QWorld;

            for (float i = Time.deltaTime; i <= 1f; i += Time.deltaTime / Duration)
            {
                player.transform.position = Vector3.Lerp(start, end, i);
                await UniTask.NextFrame();
            }

            if (NavMesh.SamplePosition(player.transform.position, out var hit, 1f, (int)NavFlags.Walkable))
                player.transform.position = hit.position;
        }

        public bool IsValid([MaybeNullWhen(true)] out string error)
        {
            error = null;
            return true;
        }

        public void DuringSceneGui(IInteractionSource source, SceneGUIProxy sceneGUI)
        {
            var previousMatrix = sceneGUI.matrix;
            sceneGUI.matrix = LocalMatrix(source.transform);
            
            P1 = sceneGUI.PositionHandle(P1, Rotation);
            P2 = sceneGUI.PositionHandle(P2, Rotation);

            Rotation = sceneGUI.RotationHandle(Rotation, P1);

            sceneGUI.matrix = previousMatrix;

            var P1World = source.transform.position + source.transform.rotation * P1;
            var P2World = source.transform.position + source.transform.rotation * P2;

            if (NavMesh.SamplePosition(P1World, out NavMeshHit hit, 1f, (int)NavFlags.Walkable))
                sceneGUI.DottedLine(P1World, hit.position, 1f);
            else
                sceneGUI.Label(P1World, "No navmesh in range", Color.red);

            if (NavMesh.SamplePosition(P2World, out hit, 1f, (int)NavFlags.Walkable))
                sceneGUI.DottedLine(P2World, hit.position, 1f);
            else
                sceneGUI.Label(P2World, "No navmesh in range", Color.red);
        }
    }
}
