using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu(" GrandMythos/UI/SelectionTracker")]
public class SelectionTracker : MonoBehaviour
{
    [Tooltip("The element that should be selected when this is opened, can be a parent containing selectable, the first one will be selected.")]
    public required GameObject DefaultSelection;

    [Tooltip("Whether selection should be set to the last element that was selected whenever this is enabled again")]
    public bool RestoreSelection = true;
    public bool WarnWhenNoSelection = true;
    readonly List<GameObject> _selectionHistory = new();

    protected virtual void Update()
    {
        var selection = EventSystem.current.currentSelectedGameObject;
        if (selection != null && selection.GetComponentInParent<SelectionTracker>() == this)
        {
            if (_selectionHistory.Count > 0 && (_selectionHistory[^1] == null || _selectionHistory[^1].transform.parent == selection.transform.parent))
            {
                _selectionHistory[^1] = selection;
            }
            else
            {
                _selectionHistory.Add(selection);
                if (_selectionHistory.Count > 50)
                    _selectionHistory.RemoveAt(0);
            }

            if (IsValidForSelection(selection.GetComponent<Selectable>()) == false)
                SelectLastActiveSelectable();
        }

        if (selection == null)
        {
            bool childrenTracker = false;
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeInHierarchy && child.GetComponentInChildren<SelectionTracker>() != null)
                {
                    childrenTracker = true;
                    break;
                }
            }
            if (childrenTracker == false)
                SelectLastActiveSelectable();
        }
    }

    protected virtual void OnEnable()
    {
        if (RestoreSelection)
            SelectLastActiveSelectable();
    }

    public void SelectLastActiveSelectable()
    {
        for (int i = _selectionHistory.Count - 1; i >= 0; i--)
        {
            if (_selectionHistory[i] != null
                && _selectionHistory[i].GetComponent<Selectable>() is {} selectable
                && IsValidForSelection(selectable))
            {
                selectable.Select();
                return;
            }
            _selectionHistory.RemoveAt(i);
        }

        {
            if (DefaultSelection != null!)
            {
                foreach (var selectable in DefaultSelection.GetComponentsInChildren<Selectable>())
                {
                    if (IsValidForSelection(selectable))
                    {
                        selectable.Select();
                        break;
                    }
                }
            }
            else if (WarnWhenNoSelection)
                Debug.LogWarning($"Could not set {nameof(SelectLastActiveSelectable)} - no history or active selectable found under {DefaultSelection}", this);
        }
    }

    static bool IsValidForSelection(Selectable? selectable)
    {
        if (selectable == null || selectable.enabled == false || selectable.IsInteractable() == false || selectable.navigation.mode == Navigation.Mode.None)
            return false;
        return selectable.gameObject.activeInHierarchy;
    }

    public void SelectPreviousActiveSelectable()
    {
        if (_selectionHistory.Count > 0)
            _selectionHistory.RemoveAt(_selectionHistory.Count - 1);
        SelectLastActiveSelectable();
    }
}