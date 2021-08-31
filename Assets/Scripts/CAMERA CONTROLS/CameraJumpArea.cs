using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraJumpArea : MonoBehaviour
{
    //VARIABLES
    public GameObject posChange;
    public Vector3 currentPos;

    [Space]
    public CameraFollowArea follow;

    //UPDATES
    //private void Start()
    //{
    //    // IF THIS IS THE FIRST CAMERA POSITION, MOVE CAM TO HERE
    //    if(posChange.CompareTag("StartCamPos"))
    //    {
    //        CutToShot();
    //    }
    //}

    //METHODS
    private void OnTriggerEnter(Collider other)
    {
       if(other.CompareTag("Player"))
        {
            // MOVE CAMERA TO POSCHANGE POSITION
            foreach (CameraFollowArea cf in FindObjectsOfType<CameraFollowArea>())
                cf.StopFollow();
            CutToShot();
        }
    }

    public void CutToShot()
    {
        Camera.main.transform.localPosition = posChange.transform.position;
        Camera.main.transform.localRotation = posChange.transform.rotation;
    }
}
