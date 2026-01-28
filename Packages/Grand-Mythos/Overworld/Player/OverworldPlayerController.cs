using System;
using System.Collections;
using System.Collections.Generic;
using Interactables;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;
using UnityEngine.InputSystem;

public class OverworldPlayerController : ReloadableBehaviour
{
    const int OffMeshLinkStart = 0;
    const int OffMeshLinkEnd = 2;

    /// <summary> Index of the character layer </summary>
    public const int CharacterLayer = 3;
    /// <summary> LayerMask containing only Characters </summary>
    public readonly LayerMask CharacterLayerMask = 1<<4;

    public static HashSet<OverworldPlayerController> Instances = new();
    static Collider[] _sphereCastUtility = new Collider[16];


    public required CharacterController Controller;

    [PropertyTooltip("Speed of the jump in meters per second")]
    public float JumpUnitPerSecond = 10f;
    public float InteractDistance = 0.5f;
    public float TurnRate = 20f;
    public float MovementSpeed = 8f;
    public ControlDisabler Disabler;
    public double UnitsWalked;

    [SerializeField, Sirenix.OdinInspector.ReadOnly]
    int _interactionStacked;

    public MeansOfTransport[] MeansOfTransports = new MeansOfTransport[]
    {
        new()
        {
            NavFlags = NavFlags.Walkable
        }
    };

    public float SwapTransportQueryRadius = 5f;
    public required InputActionReference Interact, Move;

    bool _noNavmesh;
    Vector3 _lastPointOnNavMesh;
    int _activeTransport;


    [Flags]
    public enum ControlDisabler
    {
        Jump = 0b1,
        Interacting = 0b10,
    }

    int NavMeshArea => (int)MeansOfTransports[_activeTransport].NavFlags;

    void Start()
    {
        MeansOfTransports[_activeTransport].OnActivate?.Invoke();
    }

    protected override void OnEnabled(bool afterDomainReload)
    {
        InputManager.PushGameState(GameState.Overworld, this);
        Instances.Add(this);

        _noNavmesh = false;
        var position = Controller.transform.position;
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 10f, NavMeshArea))
        {
            Controller.transform.position = hit.position;
            _lastPointOnNavMesh = hit.position;
        }
        else if (NavMesh.SamplePosition(position, out hit, float.PositiveInfinity, NavMeshArea))
        {
            Controller.transform.position = hit.position;
            _lastPointOnNavMesh = hit.position;
            Debug.LogError("No navmesh around position, teleported to closest one");
        }
        else
        {
            _noNavmesh = true;
            Debug.LogError("No navmesh found, make sure the navmesh for this scene has been built");
        }
    }

    protected override void OnDisabled(bool beforeDomainReload)
    {
        InputManager.PopGameState(this);
    }

    public void PlayInteraction(IInteractionSource source, IInteraction interaction)
    {
        _interactionStacked++;
        Disabler |= ControlDisabler.Interacting; // Just in case a coroutine does not start immediately
        GameManager.Instance.StartUndisablableCoroutine(this, InteractionRoutine(this, source, interaction));

        static IEnumerator InteractionRoutine(OverworldPlayerController @this, IInteractionSource source, IInteraction interaction)
        {
            try
            {
                foreach (var delay in interaction.InteractEnum(source, @this))
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
                @this._interactionStacked--;
                if (@this._interactionStacked == 0)
                    @this.Disabler &= ~ControlDisabler.Interacting;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (gameObject.layer != CharacterLayer)
            GizmosHelper.Label(transform.position, $"This {nameof(OverworldPlayerController)} is not on the expected layer, expected layer #{CharacterLayer}, got layer #{gameObject.layer}", Color.red);
    }

    void OnDestroy()
    {
        Instances.Remove(this);
    }


    void Update()
    {
        if (Disabler != 0)
            return;

        {
            InteractionComp? closestInteraction = null;
            float closestDist = InteractDistance;
            foreach (var interactionComp in InteractionComp.Instances)
            {
                var closestOnBounds = interactionComp.transform.position;
                var distToBounds = Vector3.Distance(closestOnBounds, transform.position);
                if (distToBounds < closestDist)
                {
                    closestDist = distToBounds;
                    closestInteraction = interactionComp;
                }
            }

            if (closestInteraction is not null)
            {
                if (Prompt.TryShowInteractivePromptThisFrame(closestInteraction.transform.position, closestInteraction.Label)
                    && Interact.action.WasPressedThisFrame())
                    closestInteraction.Trigger();
            }
        }

        {
            Interactable? closestInteractable = null;
            float closestDist = float.PositiveInfinity;
            for (int i = Physics.OverlapSphereNonAlloc(transform.position, InteractDistance, _sphereCastUtility, ~CharacterLayerMask, QueryTriggerInteraction.Collide) - 1; i >= 0; i--)
            {
                var c = _sphereCastUtility[i];
                if (c.TryGetComponent<Interactable>(out var interactable) == false || interactable.Consumed)
                    continue;

                var closestOnBounds = c.ClosestPointOnBounds(transform.position);
                var distToBounds = Vector3.Distance(closestOnBounds, transform.position);
                if (distToBounds < closestDist)
                {
                    closestDist = distToBounds;
                    closestInteractable = interactable;
                }
            }

            if (closestInteractable is not null)
            {
                if (Prompt.TryShowInteractivePromptThisFrame(closestInteractable.transform.position, closestInteractable.Text)
                    && Interact.action.WasPressedThisFrame()
                    && closestInteractable.TryConsumeAndPlayInteraction(this))
                    return;
            }
        }

        Vector3 movementVector;
        var inputMovement = Move.action.ReadValue<Vector2>();
        movementVector.x = inputMovement.x;
        movementVector.y = 0;
        movementVector.z = inputMovement.y;

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

        MoveOffset(movementVector);
        JumpLinkQuery();
        ChangeOfTransportQuery();
    }

    void MoveOffset(Vector3 movementVector)
    {
        var previousPosition = Controller.transform.position;
        try
        {
            var offset = movementVector * (MovementSpeed * Time.deltaTime);
            Controller.Move(offset);

            if (_noNavmesh)
                return;

            var nextPosition = Controller.transform.position;
            if (NavMesh.Raycast(previousPosition, nextPosition, out NavMeshHit hit, NavMeshArea))
            {
                // We hit a wall, our velocity may not have been entirely used up though, so remove what this movement consumed,
                var offsetTraversed = hit.position - previousPosition;
                offset -= offsetTraversed;
                offset = Vector3.ProjectOnPlane(offset, hit.normal); // remove any velocity that would go through the wall we just hit - only keep what slides against it

                NavMesh.Raycast(hit.position, hit.position + offset, out hit, NavMeshArea);
                // Whether we hit a wall or not, move from the position at the start of the frame to the one we could reach
                Teleport(hit.position);
                _lastPointOnNavMesh = hit.position;
            }
            else if (offset != default)
            {
                if (NavMesh.SamplePosition(nextPosition, out hit, 0.5f, NavMeshArea))
                {
                    _lastPointOnNavMesh = hit.position;
                    float deltaT = hit.position.y - nextPosition.y;
                    Controller.Move(deltaT * Vector3.up);
                }
                else
                {
                    Debug.LogWarning("Had to teleport player back onto navmesh");
                    Teleport(_lastPointOnNavMesh);
                }
            }
        }
        finally
        {
            UnitsWalked += (Controller.transform.position - previousPosition).magnitude;
        }

        if (WorldBending.ShouldWrapAround(Controller.transform.position, out var newPos))
            Teleport(newPos);
    }

    public void Teleport(Vector3 pos)
    {
        Controller.enabled = false;
        Controller.transform.position = pos;
        Controller.enabled = true;
    }

    void JumpLinkQuery()
    {
        if (ClosestNavmeshLink(transform.position, InteractDistance, out var start, out var end))
        {
            if (Prompt.TryShowInteractivePromptThisFrame(start, "Jump") == false)
                return;

            if (Interact.action.WasPressedThisFrame() == false)
                return;

            StartCoroutine(Jump(end));
        }
    }

    void ChangeOfTransportQuery()
    {
        var pos = transform.position;
        for (int i = 0; i < MeansOfTransports.Length; i++)
        {
            if (i == _activeTransport)
                continue;

            var transport = MeansOfTransports[i];
            if (!NavMesh.SamplePosition(pos, out var hit, SwapTransportQueryRadius, (int)transport.NavFlags))
                continue;

            if (Prompt.TryShowInteractivePromptThisFrame(hit.position, transport.PromptLabel) && Interact.action.WasPressedThisFrame())
            {
                MeansOfTransports[_activeTransport].OnDeactivate?.Invoke();
                _activeTransport = i;
                Controller.transform.position = hit.position;
                MeansOfTransports[_activeTransport].OnActivate?.Invoke();
                return;
            }
        }
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

    static bool ClosestNavmeshLink(Vector3 position, float maxDist, out Vector3 closestStart, out Vector3 closestEnd)
    {
        const Allocator allocator = Allocator.TempJob;

        closestEnd = closestStart = default;

        var navMeshWorld = NavMeshWorld.GetDefaultWorld();
        if (navMeshWorld.IsValid() == false)
        {
            Debug.LogError("Invalid world");
            return false;
        }

        float closestDist = maxDist;

        using var navQuery = new NavMeshQuery(navMeshWorld, allocator);
        var closestLocation = navQuery.MapLocation(position, Vector3.one, agentTypeID:0);
        const Allocator allocator1 = Allocator.TempJob;

        using var edgeVertices = new NativeArray<Vector3>(6, allocator1);
        using var neighbors = new NativeArray<PolygonId>(32, allocator1);
        using var indices = new NativeArray<byte>(neighbors.Length, allocator1);
        using var edgeVerticesForLink = new NativeArray<Vector3>(4, allocator1);

        var neighborsResult = navQuery.GetEdgesAndNeighbors(closestLocation.polygon, edgeVertices, neighbors, indices, out int verticesCount, out int neighborsTotal);
        Debug.Assert(neighborsResult == PathQueryStatus.Success, neighborsResult);
        Debug.Assert(neighborsTotal <= neighbors.Length);
        for (int i = 0; i < neighborsTotal; i++)
        {
            PolygonId neighborId = neighbors[i];
            if (navQuery.GetPolygonType(neighborId) != NavMeshPolyTypes.OffMeshConnection)
                continue;

            navQuery.GetEdgesAndNeighbors(neighborId, edgeVerticesForLink, default, default, out _, out _);

            // This stuff is not very intuitive, but according to the documentation https://docs.unity3d.com/ScriptReference/Experimental.AI.NavMeshQuery.GetEdgesAndNeighbors.html
            // "For link nodes the returned edgeVertices array contains two pairs of points at indices [0]-[1] and [2]-[3] that define the end points of the start and end edges of the link, in this order.
            // [...] For nodes added through Off-mesh Link components the pairs contain the same value in both of their elements."
            // so startVertA may very well be equal to startVertB depending on the NavMesh method used,
            // nevertheless, we'll implement logic expecting them to actually form a line segment instead of a point,
            // as considering the point as a line of zero length works just as well.

            bool isEnd = indices[i] == OffMeshLinkEnd;
            Vector3 startVertA = edgeVerticesForLink[isEnd ? 2 : 0];
            Vector3 startVertB = edgeVerticesForLink[isEnd ? 3 : 1];

            Vector3 edgeDir = startVertB - startVertA;
            Vector3 vertToPos = position - startVertA;

            Vector3 proj = Vector3.Project(vertToPos, edgeDir);
            Vector3 closestPointOnLine = startVertA + proj;
            Vector3 closestPointOnSegment;
            if (Vector3.Dot(proj, edgeDir) < 0)
                closestPointOnSegment = startVertA;
            else if (proj.sqrMagnitude > edgeDir.sqrMagnitude)
                closestPointOnSegment = startVertB;
            else
                closestPointOnSegment = closestPointOnLine;


            Vector3 endVertA = edgeVerticesForLink[isEnd ? 0 : 2];
            Vector3 endVertB = edgeVerticesForLink[isEnd ? 1 : 3];

            float distanceToJumpPoint = Vector3.Distance(closestPointOnSegment, position);
            if (distanceToJumpPoint < closestDist)
            {
                closestDist = distanceToJumpPoint;
                closestEnd = (endVertA + endVertB) / 2f;
                closestStart = closestPointOnSegment;
            }
        }

        return closestDist != maxDist;
    }
}
