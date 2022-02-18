using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Unlock : MonoBehaviour
{
    public GameObject Laser;

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        Destroy(Laser);
    }
}
