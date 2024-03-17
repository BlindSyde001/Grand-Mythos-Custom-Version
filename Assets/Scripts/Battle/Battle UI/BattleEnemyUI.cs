using UnityEngine;
using Cinemachine;

public class BattleEnemyUI : MonoBehaviour
{
    public CinemachineFreeLook cam;

    void Start()
    {
        cam = FindObjectOfType<CinemachineFreeLook>();
    }

    void LateUpdate()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
    }
}
