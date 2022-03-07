using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraJumpArea : CameraBase
{
    // VARIABLES

    // UPDATES
    private void Awake()
    {
        cameraManager = FindObjectOfType<CameraManager>();
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
        if(other.CompareTag("Player"))
        {
            ExitCamZone();
        }
    }
}
