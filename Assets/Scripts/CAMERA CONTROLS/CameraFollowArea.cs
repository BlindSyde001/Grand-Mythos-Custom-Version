using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowArea : MonoBehaviour
{
    //VARIABLES
    private GameObject player;
    private Camera cam;
    public GameObject posChange;
    public GameObject focalPoint;


    public Vector3 offset;
    public Vector3 min;
    public Vector3 max;


    [SerializeField]
    private bool isFollowing;

    //UPDATES
    private void Start()
    {
        cam = Camera.main;

        min += transform.position;
        max += transform.position;

        if (min.x > max.x)
        {
            float val = min.x;
            min.x = max.x;
            max.x = val;
        }

        if (min.y > max.y)
        {
            float val = min.y;
            min.y = max.y;
            max.y = val;
        }

        if (min.z > max.z)
        {
            float val = min.z;
            min.z = max.z;
            max.z = val;
        }
    }

    private void LateUpdate()
    {
        // Move around the defined space
        if (isFollowing)
        {
            Vector3 pos = player.transform.position + offset;
            cam.transform.position = new Vector3(Mathf.Clamp(pos.x, min.x, max.x), Mathf.Clamp(pos.y, min.y, max.y), Mathf.Clamp(pos.z, min.z, max.z));
            //cam.transform.LookAt(focalPoint.transform);
        }
    }

    //METHODS
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            player = other.gameObject;
            CutToShot();
            StartFollow();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            StopFollow();
        }
    }
    public void StartFollow()
    {
        isFollowing = true;
    }
    public void StopFollow()
    {
        isFollowing = false;
    }

    public void CutToShot()
    {
        cam.transform.localPosition = posChange.transform.position;
        cam.transform.localRotation = posChange.transform.rotation;
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 min = transform.position + this.min;
        Vector3 max = transform.position + this.max;

        if (UnityEditor.EditorApplication.isPlaying) {
            min = this.min;
            max = this.max;
        }

        Gizmos.DrawWireCube((max + min) / 2F, max - min);
    }
#endif
}
