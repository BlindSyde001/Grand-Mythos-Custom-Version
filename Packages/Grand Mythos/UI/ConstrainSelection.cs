using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConstrainSelection : MonoBehaviour
{
    static readonly List<ConstrainSelection> _instances = new();

    void Update()
    {
        if (this == _instances[^1])
            StaticUpdate(this);
    }

    static void StaticUpdate(ConstrainSelection active)
    {
        var selection = EventSystem.current.currentSelectedGameObject;
        if (selection == null || selection.GetComponentInParent<ConstrainSelection>() != active)
        {
            if (active.GetComponentInChildren<Selectable>() is {} selectable)
                selectable.Select();
            else
                EventSystem.current.SetSelectedGameObject(null);
        }
    }

    void OnEnable()
    {
        if (_instances.Contains(this) == false) // This may happen on domain reload ... I think ? I haven't confirmed
            _instances.Add(this);
    }

    void OnDisable() => _instances.Remove(this);
}