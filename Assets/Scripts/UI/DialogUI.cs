using UnityEngine;

public class DialogUI : MonoBehaviour
{
    void OnEnable()
    {
        InputManager.PushGameState(GameState.Cutscene, this);
    }

    void OnDisable()
    {
        InputManager.PopGameState(this);
    }
}
