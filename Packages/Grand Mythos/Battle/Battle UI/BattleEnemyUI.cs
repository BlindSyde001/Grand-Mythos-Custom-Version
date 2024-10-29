using UnityEngine;

public class BattleEnemyUI : MonoBehaviour
{
    void LateUpdate()
    {
        transform.rotation = Camera.main.transform.rotation;
    }
}
