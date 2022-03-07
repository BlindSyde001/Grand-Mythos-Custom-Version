using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPanArea: CameraBase
{ 
    // UPDATES
    private void Awake()
    {
        cameraManager = FindObjectOfType<CameraManager>();
    }
    private void LateUpdate()
    {
        if (AmActiveCam)
        {
            cameraManager._Camera.transform.LookAt(cameraManager.player.transform);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SetAsActiveCam(this);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ExitCamZone();
        }
    }
}
