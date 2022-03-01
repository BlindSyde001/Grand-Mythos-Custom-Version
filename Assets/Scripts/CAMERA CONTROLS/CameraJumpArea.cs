using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraJumpArea : MonoBehaviour
{
    //VARIABLES
    public GameObject camPos;
    public Transform refDirection;

    //METHODS
    private void OnTriggerEnter(Collider other)
    {
       if(other.CompareTag("Player"))
        {
            // MOVE CAMERA TO POSCHANGE POSITION
            other.GetComponent<OverworldPlayerCircuit>().referenceDirection = refDirection;
            GameManager._instance.LastKnownReferenceDirection = Camera.main.GetComponent<DirectionStorage>().ReferenceDirections.IndexOf(refDirection);
            foreach (CameraFollowArea cf in FindObjectsOfType<CameraFollowArea>())
                cf.StopFollow();
            CutToShot();
        }
    }

    public void CutToShot()
    {
        Camera.main.transform.localPosition = camPos.transform.position;
        Camera.main.transform.localRotation = camPos.transform.rotation;
    }
}
