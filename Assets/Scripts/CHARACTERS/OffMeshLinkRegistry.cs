using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;
using UnityEngine.SceneManagement;

public static class OffMeshLinkRegistry
{
    const int OffMeshLinkStart = 0;
    const int OffMeshLinkEnd = 2;

    static HashSet<OffMeshLink> _links = new();
    static bool _hasInit;

    static OffMeshLinkRegistry()
    {
        SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
        SceneManager.sceneUnloaded += SceneManagerOnsceneUnloaded;

        static void SceneManagerOnsceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            foreach (var gameObject in arg0.GetRootGameObjects())
            {
                foreach (var link in gameObject.GetComponentsInChildren<OffMeshLink>())
                {
                    _links.Add(link);
                }
            }
        }

        static void SceneManagerOnsceneUnloaded(Scene arg0)
        {
            _links = _links.Where(x => x != null).Where(x => x.gameObject.scene != arg0).ToHashSet();
        }
    }


    public static bool ClosestLink(Vector3 position, float maxDistance, out Vector3 closestStart, out Vector3 closestEnd)
    {
        // Performance of this could be seriously improved obviously, I'll leave that exercise to whenever we find that it becomes a bottleneck
        if (_hasInit == false)
        {
            _links = Object.FindObjectsOfType<OffMeshLink>(true).ToHashSet();
            _hasInit = true;
        }

        float closestDistSqrd = maxDistance*maxDistance;
        closestEnd = closestStart = default;

        foreach (var offMeshLink in _links)
        {
            var start = offMeshLink.startTransform.position;

            var distSqrd = (start - position).sqrMagnitude;
            if (distSqrd < closestDistSqrd)
            {
                closestStart = start;
                closestEnd = offMeshLink.endTransform.position;
                closestDistSqrd = distSqrd;
            }

            if (offMeshLink.biDirectional)
            {
                var end = offMeshLink.endTransform.position;
                distSqrd = (end - position).sqrMagnitude;
                if (distSqrd < closestDistSqrd)
                {
                    closestStart = end;
                    closestEnd = start;
                    closestDistSqrd = distSqrd;
                }
            }
        }

        return closestDistSqrd < maxDistance * maxDistance;
    }

    static bool ClosestNavmeshPoint(Vector3 position, out Vector3 closestStart, out Vector3 closestEnd)
    {
        const Allocator allocator = Allocator.TempJob;

        closestEnd = closestStart = default;

        var navMeshWorld = NavMeshWorld.GetDefaultWorld();
        if (navMeshWorld.IsValid() == false)
        {
            Debug.LogError("Invalid world");
            return false;
        }

        float closestDist = float.PositiveInfinity;

        var navQuery = new NavMeshQuery(navMeshWorld, allocator);
        try
        {
            var closestLocation = navQuery.MapLocation(position, Vector3.one, agentTypeID:0);
            ClosestNavmeshPoint(ref closestLocation, ref navQuery, ref closestDist, ref closestStart, ref closestEnd, position, true);
        }
        finally
        {
            navQuery.Dispose();
        }

        return closestDist != float.PositiveInfinity;
    }

    static void ClosestNavmeshPoint(ref NavMeshLocation closestLocation, ref NavMeshQuery navQuery, ref float closestDist, ref Vector3 closestStart, ref Vector3 closestEnd, Vector3 position, bool recurseOnce)
    {
        const Allocator allocator = Allocator.TempJob;

        using var edgeVertices = new NativeArray<Vector3>(6, allocator);
        using var neighbors = new NativeArray<PolygonId>(32, allocator);
        using var indices = new NativeArray<byte>(neighbors.Length, allocator);
        using var edgeVerticesForLink = new NativeArray<Vector3>(4, allocator);

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


            Vector3 endVertA = edgeVerticesForLink[isEnd ? 0 : 2];
            Vector3 endVertB = edgeVerticesForLink[isEnd ? 1 : 3];

            Debug.DrawLine(closestPointOnSegment, endVertA, Color.green);

            float distanceToJumpPoint = Vector3.Distance(closestPointOnSegment, position);
            if (distanceToJumpPoint < closestDist)
            {
                closestDist = distanceToJumpPoint;
                closestEnd = (endVertA + endVertB) / 2f;
                closestStart = closestPointOnSegment;
            }
        }

        if (recurseOnce) // Check neighbours, this is a garbage workaround to ensure we find end points from bidirectional links - they aren't added as OffMeshConnection to the surface under the end point ...
        {
            for (int i = 0; i < neighborsTotal; i++)
            {
                PolygonId neighborId = neighbors[i];
                if (navQuery.GetPolygonType(neighborId) == NavMeshPolyTypes.Ground)
                {
                    ClosestNavmeshPoint(ref closestLocation, ref navQuery, ref closestDist, ref closestStart, ref closestEnd, position, false);
                }
            }
        }
    }
}