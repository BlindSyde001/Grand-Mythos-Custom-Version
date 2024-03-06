using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DefaultSelection : MonoBehaviour
{
    [InfoBox("Will select this element if there ever is a point where the selection is not set or is on an inactive object")]
    [Required]
    public Selectable Element;

    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.activeInHierarchy == false)
            Element.Select();
    }
}
