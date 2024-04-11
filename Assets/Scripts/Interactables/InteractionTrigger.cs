using System;
using Sirenix.OdinInspector;
using UnityEngine;

[AddComponentMenu(" GrandMythos/InteractionTrigger")]
public class InteractionTrigger : MonoBehaviour, IInteractionSource
{
    public TriggerType Type = TriggerType.OnceEveryLoad;
    [Required, SerializeReference] public IInteraction Interaction = null;

    public enum TriggerType
    {
        Always,
        OnceEveryLoad,
        OnceEver
    }

    // Show the disable/enable toggle in editor
    void OnEnable(){}
    void OnDisable(){}

    void OnDrawGizmos()
    {
        if (Interaction == null)
            GizmosHelper.Label(transform.position, $"No interaction set on this {nameof(Interactable)}", Color.red);
        else if (Interaction.IsValid(out string error) == false)
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
        if (enabled == false)
            return;

        if (other.gameObject.layer != OverworldPlayerController.CharacterLayer)
            return;

        if (other.GetComponentInParent<OverworldPlayerController>() is not { } controller)
            return;

        if (controller.TryPlayInteraction(this, Interaction))
        {
            switch (Type)
            {
                case TriggerType.Always:
                    break;
                case TriggerType.OnceEveryLoad:
                    enabled = false;
                    break;
                case TriggerType.OnceEver:
                    #warning save and load this trigger as being triggered
                    enabled = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(Type.ToString());
            }
        }
    }
}