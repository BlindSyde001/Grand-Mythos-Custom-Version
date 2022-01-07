using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Doorway : MonoBehaviour
{
    // VARIABKES
    public string SceneToLoad;
    public int NextAreasSpawnPoint; // Point that corresponds to SceneInformations DoorwayPoints so you know where you spawn

    // METHODS
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            SceneChangeManager._instance.DoorwayToSpawn = NextAreasSpawnPoint;
            SceneChangeManager._instance.LoadNewZone(SceneToLoad);
        }
    }
}
