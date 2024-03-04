using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    public SpawnPointReference NewGameScene;

    void OnEnable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.PushGameState(GameState.Title, this);
    }

    void OnDisable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.PopGameState(this);
    }

    public void NewGame() => NewGameScene.SwapSceneToThisSpawnPoint();
}
