using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Doorway : MonoBehaviour
{
    // VARIABKES
    public string SceneToLoad;

    // METHODS
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            SceneChangeManager._instance.PreviousScene = SceneManager.GetActiveScene().name;
            SceneChangeManager._instance.LoadNewZone(SceneToLoad);
        }
    }
}
