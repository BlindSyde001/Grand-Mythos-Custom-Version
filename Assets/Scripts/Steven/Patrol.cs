using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : MonoBehaviour
{
    public Transform[] checkpoints;
    public int speed;

    private int checkpointIndex;
    private float dist;


    // Start is called before the first frame update
    void Start()
    {
        checkpointIndex = 0;
        transform.LookAt(checkpoints[checkpointIndex].position);
    }

    // Update is called once per frame
    void Update()
    {
        dist = Vector3.Distance(transform.position, checkpoints[checkpointIndex].position);
        if (dist < 1f)
        {
            IncreaseIndex();
        }

        Cycle();

    }

    void Cycle()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

    }

    void IncreaseIndex()
    {
        checkpointIndex++;
        if(checkpointIndex>= checkpoints.Length)
        {
            checkpointIndex = 0;
        }

        transform.LookAt(checkpoints[checkpointIndex].position);
    }
}
