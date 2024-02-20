using System;
using System.Collections;
using System.Collections.Generic;
using Interactables;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;

public class OverworldPlayerControlsNode : ReloadableBehaviour
{
    /// <summary> Index of the character layer </summary>
    public const int CharacterLayer = 3;
    /// <summary> LayerMask containing only Characters </summary>
    public readonly LayerMask CharacterLayerMask = 1<<4;

    const int OffMeshLinkStart = 0;
    const int OffMeshLinkEnd = 2;

    public static HashSet<OverworldPlayerControlsNode> Instances = new();
    static Collider[] _sphereCastUtility = new Collider[16];



    public Animator Animator;
    public CharacterController Controller;

    [PropertyTooltip("Speed of the jump in meters per second")]
    public float JumpUnitPerSecond = 10f;
    public float InteractDistance = 0.5f;
    public float TurnRate = 20f;
    public float MovementSpeed = 8f;
    public ControlDisabler Disabler;
    public double UnitsWalked;

    PlayerControls playerControls;

    bool isLocked;

    bool noNavmesh;
    Vector3 lastPointOnNavMesh;


    [Flags]
    public enum ControlDisabler
    {
        Jump = 0b1,
        Interacting = 0b10,
    }

    protected override void OnEnabled(bool afterDomainReload)
    {
        Instances.Add(this);

        var position = Controller.transform.position;
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            Controller.transform.position = hit.position;
            lastPointOnNavMesh = hit.position;
        }
        else
            Debug.LogError("No navmesh found, make sure the navmesh for this scene has been built");

        playerControls = new PlayerControls();
        playerControls.Enable();
    }

    public bool TryPlayInteraction(IInteractionSource source, IInteraction interaction)
    {
        if ((Disabler & ControlDisabler.Interacting) != 0)
            return false;

        Disabler |= ControlDisabler.Interacting; // Just in case coroutine does not start immediately
        StartCoroutine(InteractionRoutine(source, interaction));
        return true;
    }

    void OnDrawGizmosSelected()
    {
        if (gameObject.layer != CharacterLayer)
            GizmosHelper.Label(transform.position, $"This {nameof(OverworldPlayerControlsNode)} is not on the expected layer, expected layer #{CharacterLayer}, got layer #{gameObject.layer}", Color.red);
    }

    protected override void OnDisabled(bool beforeDomainReload)
    {
        playerControls.Disable();
    }

    void OnDestroy()
    {
        Instances.Remove(this);
    }

    void SetMoving(bool state)
    {
        if(state)
        {
            Animator.Play("Run");
        }
        else
        {
            Animator.Play("Idle");
        }
    }


    void LateUpdate()
    {
        if (Disabler != 0)
        {
            SetMoving(false);
            return;
        }

        for (int i = Physics.OverlapSphereNonAlloc(transform.position, InteractDistance, _sphereCastUtility, ~CharacterLayerMask, QueryTriggerInteraction.Collide) - 1; i >= 0; i--)
        {
            Collider c = _sphereCastUtility[i];
            if (c.GetComponent<Interactable>() is not Interactable interactable)
                continue;

            Prompt.TryShowPromptThisFrame(interactable.transform.position, interactable.Text);
            if (interactable.Interaction == null)
            {
                Debug.LogError($"No interaction on this interactable ({interactable})", interactable);
                continue;
            }

            if (playerControls.OverworldMap.Interact.WasPressedThisFrame() && TryPlayInteraction(interactable, interactable.Interaction))
                return;
        }

        if (playerControls.OverworldMap.OpenMenu.IsPressed())
        {
            SetMoving(false);
            return;
        }

        Vector3 movementVector;
        if (playerControls.OverworldMap.Move.IsPressed())
        {
            SetMoving(true);
            var inputMovement = playerControls.OverworldMap.Move.ReadValue<Vector2>();
            movementVector.x = inputMovement.x;
            movementVector.y = 0;
            movementVector.z = inputMovement.y;
        }
        else
        {
            SetMoving(false);
            movementVector = default;
        }

        Vector3 facingDirection = Vector3.Cross(Camera.main.transform.right, Vector3.up).normalized;
        var inputSpace = Quaternion.LookRotation(facingDirection);
        if (movementVector != default)
        {
            movementVector = inputSpace * movementVector;
            movementVector.y = 0f;

            if (movementVector.magnitude > 1f)
                movementVector.Normalize(); // Normalize so movement in all directions is same speed

            var targetRotation = Quaternion.Euler(0, Mathf.Rad2Deg * Mathf.Atan2(movementVector.x, movementVector.z), 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, TurnRate * Time.deltaTime);
        }

        Move(movementVector);
        JumpLinkQuery();
        #warning still have to deal with menu
    }

    void Move(Vector3 movementVector)
    {
        var previousPosition = Controller.transform.position;
        try
        {
            var offset = movementVector * (MovementSpeed * Time.deltaTime);
            Controller.Move(offset);

            if (noNavmesh)
                return;

            var nextPosition = Controller.transform.position;
            if (NavMesh.Raycast(previousPosition, nextPosition, out NavMeshHit hit, NavMesh.AllAreas))
            {
                // We hit a wall, our velocity may not have been entirely used up though, so remove what this movement consumed,
                var offsetTraversed = hit.position - previousPosition;
                offset -= offsetTraversed;
                offset = Vector3.ProjectOnPlane(offset, hit.normal); // remove any velocity that would go through the wall we just hit - only keep what slides against it

                NavMesh.Raycast(hit.position, hit.position + offset, out hit, NavMesh.AllAreas);
                // Whether we hit a wall or not, move from the position at the start of the frame to the one we could reach
                transform.position = hit.position;
                lastPointOnNavMesh = hit.position;
            }
            else if (offset != default)
            {
                if (NavMesh.SamplePosition(nextPosition, out hit, 0.5f, NavMesh.AllAreas))
                {
                    lastPointOnNavMesh = hit.position;
                }
                else
                {
                    Debug.LogWarning("Had to teleport player back onto navmesh");
                    transform.position = lastPointOnNavMesh;
                }
            }
        }
        finally
        {
            UnitsWalked += (Controller.transform.position - previousPosition).magnitude;
        }
    }

    void JumpLinkQuery()
    {
        const Allocator allocator = Allocator.TempJob;

        var position = Controller.transform.position;
        var navMeshWorld = NavMeshWorld.GetDefaultWorld();
        if (navMeshWorld.IsValid() == false)
        {
            Debug.LogError("Invalid world");
            return;
        }

        using var edgeVertices = new NativeArray<Vector3>(6, allocator);
        using var neighbors = new NativeArray<PolygonId>(32, allocator);
        using var indices = new NativeArray<byte>(neighbors.Length, allocator);
        using var edgeVerticesForLink = new NativeArray<Vector3>(4, allocator);

        using var navQuery = new NavMeshQuery(navMeshWorld, allocator);
        var closestLocation = navQuery.MapLocation(position, Vector3.one, agentTypeID:0);
        var neighborsResult = navQuery.GetEdgesAndNeighbors(closestLocation.polygon, edgeVertices, neighbors, indices, out int verticesCount, out int neighborsTotal);

        Debug.Assert(neighborsResult == PathQueryStatus.Success, neighborsResult);
        if (neighborsResult != PathQueryStatus.Success)
            return;

        float closestDist = float.PositiveInfinity;
        Vector3 closestStart = default, closestEnd = default;
        Debug.Assert(neighborsTotal <= neighbors.Length);
        for (int i = 0; i < neighborsTotal; i++)
        {
            PolygonId neighborId = neighbors[i];
            if (navQuery.GetPolygonType(neighborId) != NavMeshPolyTypes.OffMeshConnection)
                continue;

            if (indices[i] == OffMeshLinkEnd)
                continue;

            navQuery.GetEdgesAndNeighbors(neighborId, edgeVerticesForLink, default, default, out _, out _);

            // This stuff is not very intuitive, but according to the documentation https://docs.unity3d.com/ScriptReference/Experimental.AI.NavMeshQuery.GetEdgesAndNeighbors.html
            // "For link nodes the returned edgeVertices array contains two pairs of points at indices [0]-[1] and [2]-[3] that define the end points of the start and end edges of the link, in this order.
            // [...] For nodes added through Off-mesh Link components the pairs contain the same value in both of their elements."
            // so startVertA may very well be equal to startVertB depending on the NavMesh method used,
            // nevertheless, we'll implement logic expecting them to actually form a line segment instead of a point,
            // as considering the point as a line of zero length works just as well.

            Vector3 startVertA = edgeVerticesForLink[0];
            Vector3 startVertB = edgeVerticesForLink[1];

            Vector3 edgeDir = startVertA - startVertB;
            Vector3 vertToPos = startVertA - position;

            Vector3 closestPointOnLine = position + vertToPos - Vector3.Project(vertToPos, edgeDir);
            Vector3 deltaToVert = startVertA - closestPointOnLine;
            float dot = Vector3.Dot(deltaToVert, edgeDir);
            Vector3 closestPointOnSegment;
            if (dot < 0)
                closestPointOnSegment = startVertA;
            else if (dot > 1)
                closestPointOnSegment = startVertB;
            else
                closestPointOnSegment = closestPointOnLine;
            Debug.DrawRay(startVertA, Vector3.up, Color.blue);
            Debug.DrawRay(startVertB, Vector3.up, Color.blue);
            Debug.DrawLine(startVertA, startVertB, Color.blue);
            Debug.DrawLine(closestPointOnSegment, position, Color.red);


            Vector3 endVertA = edgeVerticesForLink[2];
            Vector3 endVertB = edgeVerticesForLink[3];

            Debug.DrawLine(closestPointOnSegment, endVertA, Color.green);

            float distanceToJumpPoint = Vector3.Distance(closestPointOnSegment, position);
            if (distanceToJumpPoint >= InteractDistance)
                continue;

            if (distanceToJumpPoint < closestDist)
            {
                closestDist = distanceToJumpPoint;
                closestEnd = (endVertA + endVertB) / 2f;
                closestStart = closestPointOnSegment;
            }
        }

        if (closestDist == float.PositiveInfinity)
            return;

        Prompt.TryShowPromptThisFrame(closestStart, "Jump");

        if (playerControls.OverworldMap.Interact.WasPressedThisFrame() == false)
            return;

        StartCoroutine(Jump(closestEnd));
    }

    IEnumerator Jump(Vector3 destination)
    {
        Disabler |= ControlDisabler.Jump;

        try
        {
            Vector3 origin = Controller.transform.position;
            float distance = Vector3.Distance(origin, destination);
            for (float f = 0; f < 1f; )
            {
                yield return null;
                f += Time.deltaTime / distance * JumpUnitPerSecond;
                Controller.Move(Vector3.Lerp(origin, destination, f) - Controller.transform.position);
            }
        }
        finally
        {
            Disabler &= ~ControlDisabler.Jump;
        }
    }

    IEnumerator InteractionRoutine(IInteractionSource source, IInteraction interaction)
    {
        Disabler |= ControlDisabler.Interacting;
        try
        {
            foreach (var delay in interaction.Interact(source, this))
            {
                switch (delay)
                {
                    case Delay.WaitTillNextFrame:
                        yield return null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        finally
        {
            Disabler &= ~ControlDisabler.Interacting;
        }
    }
}
