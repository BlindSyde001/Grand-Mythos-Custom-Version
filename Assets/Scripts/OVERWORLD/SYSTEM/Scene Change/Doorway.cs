using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doorway : MonoBehaviour
{
    // VARIABKES
    public string SceneToLoad;
    private int SpawnIndex;

    // METHODS
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            FindObjectOfType<SceneChangeManager>().SceneToLoad = SceneToLoad;
            FindObjectOfType<SceneChangeManager>().DoorwayIndex = SpawnIndex;
        }
    }
}
