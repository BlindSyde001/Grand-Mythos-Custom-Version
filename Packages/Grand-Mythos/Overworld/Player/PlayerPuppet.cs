using System;
using System.Linq;
using UnityEngine;

public class PlayerPuppet : MonoBehaviour
{
    private OverworldPlayerController[] tracked = Array.Empty<OverworldPlayerController>();
    
    private void OnEnable()
    {
        tracked = OverworldPlayerController.Instances.ToArray();
        foreach (var controller in tracked)
        {
            transform.position = controller.gameObject.transform.position;
            transform.rotation = controller.gameObject.transform.rotation;
            controller.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        foreach (var controller in tracked)
        {
            controller.gameObject.transform.position = transform.position;
            controller.gameObject.transform.rotation = transform.rotation;
            controller.gameObject.SetActive(true);
        }
    }
}
