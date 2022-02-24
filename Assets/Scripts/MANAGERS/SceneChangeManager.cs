using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeManager : MonoBehaviour
{
    // VARIABLES
    public static SceneChangeManager _instance;
    public int DoorwayToSpawn;

    // UPDATES
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }

    // METHODS
    public void LoadNewZone(string NewScene)
    {
        SceneManager.LoadScene(NewScene);
    }
}
