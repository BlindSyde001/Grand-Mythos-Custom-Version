using System;
using Sirenix.OdinInspector;
using UnityEngine;

[AddComponentMenu(" GrandMythos/InteractionTrigger")]
public class InteractionTrigger : UniqueInteractionSource
{
    // Show the disable/enable toggle in editor
    void OnEnable(){}
    void OnDisable(){}

    void OnDrawGizmos()
    {
        if (OnTrigger == null)
            GizmosHelper.Label(transform.position, $"No interaction set on this {nameof(Interactable)}", Color.red);
        else if (OnTrigger.IsValid(out string error) == false)
            GizmosHelper.Label(transform.position, error, Color.red);
        else if (GetComponent<Collider>() is Collider c && c != null)
        {
            if (c.isTrigger == false)
                GizmosHelper.Label(transform.position, "Set this collider to trigger", Color.red);
        }
        else
            GizmosHelper.Label(transform.position, $"Add a collider to this {nameof(Interactable)}", Color.red);
    }

    void OnTriggerEnter(Collider other)
    {
        if (Type is TriggerType.OnceEver or TriggerType.OnceEveryLoad && Consumed)
            return;

        if (enabled == false)
            return;

        if (other.gameObject.layer != OverworldPlayerController.CharacterLayer)
            return;

        if (other.GetComponentInParent<OverworldPlayerController>() is not { } controller)
            return;

        if (controller.TryPlayInteraction(this))
        {
            switch (Type)
            {
                case TriggerType.Always:
                    break;
                case TriggerType.OnceEveryLoad:
                case TriggerType.OnceEver:
                    enabled = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(Type.ToString());
            }
        }
    }
}