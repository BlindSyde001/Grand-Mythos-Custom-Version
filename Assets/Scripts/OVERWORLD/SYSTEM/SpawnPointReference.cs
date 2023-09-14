using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnPointReference : ScriptableObject
{
    public string SpawnName;
    public SpawnablePlayerCharacter PlayerCharacter;
    public SceneReference Scene;

    public void SwapSceneToThisSpawnPoint()
    {
        SceneManager.LoadScene(Scene.Path);
        SpawnPoint.ScheduledSpawnOnPoint = this;
    }
}