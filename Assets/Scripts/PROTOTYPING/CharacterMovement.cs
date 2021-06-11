using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{
    // Move from point you are standing, to a point in your proximity.
    // Fire ray downwards to pinpoint your location
    // Fire ray at a point around you downwards
    // If moveable area, go there
    // If not, repeat step 3

    // find XZ within a radius value around character.

    public float minRadius;
    public float maxRadius;
    public Vector2 point;

    internal NavMeshAgent agent;
    [SerializeField]
    internal Transform lookTarget;

    #region Timer
    float t = 3;
    float tick = 0;
    #endregion

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    private void Update()
    {
        tick += Time.deltaTime;
        if(tick >= t)
        {
            MoveToNewPoint();
            tick = 0;
        }
        if (lookTarget != null)
            transform.LookAt(lookTarget);
    }
    public void MoveToNewPoint()
    {
        point = (Random.insideUnitCircle.normalized * Random.Range(minRadius, maxRadius)) 
                + new Vector2(transform.position.x, transform.position.z);
        
        agent.SetDestination(new Vector3(point.x, 0, point.y));
    }
}