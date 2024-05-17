using System.Linq;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

[AddComponentMenu(" GrandMythos/RunDialogComponent")]
public class RunDialogComponent : MonoBehaviour
{
    [Required]
    public ScriptMachine DialogScript;

    void Start()
    {
        EventBus.Trigger(new EventHook(DialogTriggeredEvent.Key, DialogScript.gameObject), DialogScript.gameObject);
    }

    void OnValidate()
    {
        if (DialogScript?.graph?.units.FirstOrDefault(x => x is DialogTriggeredEvent) == null)
            Debug.LogError($"This component cannot trigger the dialog as {DialogScript}'s graph doesn't have a {nameof(DialogTriggeredEvent)}");
    }
}