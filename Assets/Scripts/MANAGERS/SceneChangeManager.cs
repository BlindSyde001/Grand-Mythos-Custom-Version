using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeManager : MonoBehaviour
{
    // VARIABLES
    public int DoorwayIndex;
    public string SceneToLoad;

    // UPDATES

    // METHODS
    public void LoadNewZone(string newScene, int index)
    {
        SceneManager.LoadScene(SceneToLoad);
    }

}
