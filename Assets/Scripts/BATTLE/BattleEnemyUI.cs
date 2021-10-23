using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEnemyUI : MonoBehaviour
{
    private void LateUpdate()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }
}
