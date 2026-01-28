using UnityEngine.SceneManagement;

public class SpawnPointReference : IdentifiableScriptableObject
{
    public string SpawnName = "";
    public required SpawnablePlayerCharacter PlayerCharacter;
    public required SceneReference Scene;

    public void SwapSceneToThisSpawnPoint()
    {
        SceneManager.LoadScene(Scene.Path);
        SpawnPoint.ScheduledSpawnOnPoint = this;
    }
}