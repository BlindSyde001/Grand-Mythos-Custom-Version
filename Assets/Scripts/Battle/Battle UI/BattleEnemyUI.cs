using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class BattleEnemyUI : MonoBehaviour
{
    public CinemachineFreeLook cam;

    private void Start()
    {
        cam = FindObjectOfType<CinemachineFreeLook>();
    }
    private void LateUpdate()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
    }
}
