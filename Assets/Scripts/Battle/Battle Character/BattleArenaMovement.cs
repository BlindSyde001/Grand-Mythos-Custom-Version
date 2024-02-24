using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BattleArenaMovement : MonoBehaviour
{
    // Move from point you are standing, to a point in your proximity.
    // Fire ray downwards to pinpoint your location
    // Fire ray at a point around you downwards
    // If moveable area, go there
    // If not, repeat step 3

    // find XZ within a radius value around character.

    // VARIABLES
    public float minRadius;
    public float maxRadius;
    public Vector2 point;
    [SerializeField]
    internal NavMeshAgent agent;
    [SerializeField]
    internal Transform myTarget;
    [SerializeField]
    internal bool isRoaming;
    #region Timer
    float t = 5;
    float tick = 0;
    #endregion

    // UPDATES
    void Update()
    {
        if (isRoaming)
        {
            //MoveAroundArena();
        }
    }

    // METHODS
    public void MoveAroundArena()
    {
        tick += Time.deltaTime;
        if (tick >= t)
        {
            MoveToNewPoint();
            tick = 0;
        }
        if (myTarget != null)
            transform.LookAt(myTarget);
    }
    public void MoveToNewPoint()
    {
        point = (Random.insideUnitCircle.normalized * Random.Range(minRadius, maxRadius))
                + new Vector2(transform.position.x, transform.position.z);

        agent.SetDestination(new Vector3(point.x, 0, point.y));
    }
}
