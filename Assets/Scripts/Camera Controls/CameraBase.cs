using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBase : MonoBehaviour
{
    private protected CameraManager cameraManager;
    private protected bool AmActiveCam;

    public GameObject camPos;
    public Transform refDirection;

    public void SetAsActiveCam(CameraBase cBase) // Set As Active Cameraa
    {
        if(cameraManager.previousCameraSetup == null)
        {
            cameraManager.previousCameraSetup = cBase;
        }
        if(cameraManager.currentCameraSetup != null)
        {
            cameraManager.currentCameraSetup.AmActiveCam = false;
        }

        cameraManager.currentCameraSetup = cBase;
        cBase.AmActiveCam = true;
        CutToShot(cBase);
        cameraManager.player.GetComponent<OverworldPlayerCircuit>().referenceDirection = cBase.refDirection;
        GameManager._instance.LastKnownReferenceDirection = cameraManager.ReferenceDirections.IndexOf(cBase.refDirection);
    }
    public void ExitCamZone()
    {
        // If I Exit the Previous
        if(cameraManager.previousCameraSetup == this)
        {   // Discard Previous setup
            cameraManager.previousCameraSetup = cameraManager.currentCameraSetup;
        }
        // If I Exit the Current
        else if(cameraManager.currentCameraSetup == this)
        {   // Go back to the Previous Cam Setup
            SetAsActiveCam(cameraManager.previousCameraSetup);
            cameraManager.previousCameraSetup = cameraManager.currentCameraSetup;
        }
        AmActiveCam = false;
    }
    public void CutToShot(CameraBase cbase)
    {
        cameraManager._Camera.transform.localPosition = cbase.camPos.transform.position;
        cameraManager._Camera.transform.localRotation = cbase.camPos.transform.rotation;
    }
}
