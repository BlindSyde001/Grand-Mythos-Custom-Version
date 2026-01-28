using UnityEngine;

[AddComponentMenu(" GrandMythos/Interactable")]
public class Interactable : UniqueInteractionSource
{
    [Space]
    public string Text = "Interact?";

    void OnDrawGizmos()
    {
        if (OnTrigger == null!)
            GizmosHelper.Label(transform.position, $"No interaction set on this {nameof(Interactable)}", Color.red);
        else if (OnTrigger.IsValid(out var error) == false)
            GizmosHelper.Label(transform.position, error, Color.red);
        else if (TryGetComponent<Collider>(out var c))
        {
            if (c.isTrigger == false)
                GizmosHelper.Label(transform.position, "Set this collider to trigger", Color.red);
        }
        else
            GizmosHelper.Label(transform.position, $"Add a collider to this {nameof(Interactable)}", Color.red);
    }
}