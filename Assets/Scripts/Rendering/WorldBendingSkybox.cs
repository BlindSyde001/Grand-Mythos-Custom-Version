using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu(" GrandMythos/WorldBendingSkybox")]
public class WorldBendingSkybox : MonoBehaviour
{
    void OnEnable()
    {
        Camera.onPreCull += OnAnyCameraPreCull;
    }

    void OnDisable()
    {
        Camera.onPreCull -= OnAnyCameraPreCull;
    }

    void OnAnyCameraPreCull(Camera cam)
    {
        this.transform.position = cam.transform.position;
    }
}