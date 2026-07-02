using System;
using System.Linq;
using UnityEngine;

public class PlayerPuppet : MonoBehaviour
{
    private OverworldPlayerController[] tracked = Array.Empty<OverworldPlayerController>();
    
    private void OnEnable()
    {
        if (BaseEncounter.SetActiveAsPartOfEncounter)
            return;

        tracked = OverworldPlayerController.Instances.ToArray();
        foreach (var controller in tracked)
        {
            transform.position = controller.transform.position;
            transform.rotation = controller.transform.rotation;
            controller.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (BaseEncounter.SetActiveAsPartOfEncounter)
            return;

        if (gameObject.scene.isLoaded == false)
            return; // Do not trigger when exiting out of play mode

        foreach (var controller in tracked)
        {
            if (controller == null)
                continue;

            controller.transform.position = transform.position;
            controller.transform.rotation = transform.rotation;
            controller.gameObject.SetActive(true);
        }
    }
}
