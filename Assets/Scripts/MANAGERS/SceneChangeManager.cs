using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeManager : MonoBehaviour
{
    // VARIABLES
    public string PreviousScene;

    // METHODS
    public void LoadNewZone(string NewScene)
    {
        SceneManager.LoadScene(NewScene);
    }
}
