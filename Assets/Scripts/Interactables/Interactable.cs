using Sirenix.OdinInspector;
using UnityEngine;

[AddComponentMenu(" GrandMythos/Interactable")]
public class Interactable : MonoBehaviour, IInteractionSource
{
    public string Text = "Interact?";
    [Required, SerializeReference] public IInteraction Interaction = null;

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
}