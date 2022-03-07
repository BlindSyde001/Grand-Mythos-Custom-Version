using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowArea : CameraBase
{
    //VARIABLES
    public Vector3 offset;
    public Vector3 min;
    public Vector3 max;

    [SerializeField]
    private bool lookAtPlayer;

    //UPDATES
    private void Awake()
    {
        cameraManager = FindObjectOfType<CameraManager>();
    }
    private void Start()
    {
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
        if (AmActiveCam)
        {
            if (cameraManager.player != null)
            {
                Vector3 pos = cameraManager.player.transform.position + offset;
                cameraManager._Camera.transform.position = new Vector3(Mathf.Clamp(pos.x, min.x, max.x), Mathf.Clamp(pos.y, min.y, max.y), Mathf.Clamp(pos.z, min.z, max.z));
                if (lookAtPlayer)
                {
                    cameraManager._Camera.transform.LookAt(cameraManager.player.transform);
                }
            }
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

    //METHODS
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 min = transform.position + this.min;
        Vector3 max = transform.position + this.max;

        if (UnityEditor.EditorApplication.isPlaying)
        {
            min = this.min;
            max = this.max;
        }

        Gizmos.DrawWireCube((max + min) / 2F, max - min);
    }
#endif
}
