using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    public SpawnPointReference NewGameScene;

    void OnEnable()
    {
        InputManager.PushGameState(GameState.Menu, this);
    }

    void OnDisable()
    {
        InputManager.PopGameState(this);
    }

    public void NewGame() => NewGameScene.SwapSceneToThisSpawnPoint();

    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
