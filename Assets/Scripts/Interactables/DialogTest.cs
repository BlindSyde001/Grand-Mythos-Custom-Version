using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

public class DialogTest : MonoBehaviour
{
    [Required]
    public ScriptMachine DialogScript;

    void Start()
    {
        EventBus.Trigger(new EventHook(DialogTriggeredEvent.Key, DialogScript.gameObject), DialogScript.gameObject);
    }
}