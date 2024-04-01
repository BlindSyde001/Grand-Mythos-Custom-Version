using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu(" GrandMythos/UI/SelectionTracker")]
public class SelectionTracker : MonoBehaviour
{
    [InfoBox("The element that should be selected when this is opened, can be a parent containing selectable, the first one will be selected.")]
    [Required] public GameObject DefaultSelection;

    [InfoBox("Whether selection should be set to the last element that was selected whenever this is enabled again")]
    public bool RestoreSelection = true;
    readonly List<GameObject> _selectionHistory = new();

    protected virtual void Update()
    {
        var selection = EventSystem.current.currentSelectedGameObject;
        if (selection != null && selection.GetComponentInParent<SelectionTracker>() == this)
        {
            if (_selectionHistory.Count > 0 && _selectionHistory[^1].transform.parent == selection.transform.parent)
            {
                _selectionHistory[^1] = selection;
            }
            else
            {
                _selectionHistory.Add(selection);
                if (_selectionHistory.Count > 50)
                    _selectionHistory.RemoveAt(0);
            }
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
                && selectable != null
                && selectable.enabled
                && selectable.interactable)
            {
                selectable.Select();
                return;
            }
            _selectionHistory.RemoveAt(i);
        }

        {
            if (DefaultSelection != null && DefaultSelection.GetComponentInChildren<Selectable>() is {} selectable && selectable)
                selectable.Select();
            else
                Debug.LogWarning($"Could not set {nameof(SelectLastActiveSelectable)} - no history or active selectable found under {DefaultSelection}");
        }
    }

    public void SelectPreviousActiveSelectable()
    {
        if (_selectionHistory.Count > 0)
            _selectionHistory.RemoveAt(_selectionHistory.Count - 1);
        SelectLastActiveSelectable();
    }
}