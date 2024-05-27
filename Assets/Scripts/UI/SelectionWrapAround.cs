using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

[AddComponentMenu(" GrandMythos/UI/SelectionWrapAround")]
public class SelectionWrapAround : MonoBehaviour
{
    GameObject _previous;

    void Update()
    {
        if (EventSystem.current == null)
            return;

        var module = (InputSystemUIInputModule)EventSystem.current.currentInputModule;
        var current = EventSystem.current.currentSelectedGameObject;
        if (module.move != null && module.move.action.WasPressedThisFrame() && _previous == current && current != null && current.GetComponentInParent<SelectionWrapAround>() == this)
        {
            // When the user pressed a navigation button and navigation wasn't able to find a new selectable in that direction, wrap around the other way;
            // e.g.: if the user pressed the up direction, select the down most item by going through all selectable below us until there isn't any left below

            var currentSelectable = current.GetComponent<Selectable>();
            var direction = (Vector3)module.move.action.ReadValue<Vector2>();
            direction = -direction; // Note the inverse direction here, we're searching for the down-most one when pressing up

            var processed = new HashSet<Selectable>{ currentSelectable }; // Prevent infinite loop that may arise when wrap around is already defined through explicit selections
            for (; currentSelectable.FindSelectable(direction) is { } nextSelectable && processed.Add(nextSelectable); )
                currentSelectable = nextSelectable;

            currentSelectable.Select();
        }

        _previous = EventSystem.current.currentSelectedGameObject;
    }
}