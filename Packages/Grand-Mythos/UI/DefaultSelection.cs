using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu(" GrandMythos/UI/DefaultSelection")]
public class DefaultSelection : MonoBehaviour
{
    [Tooltip("Will select this element if there ever is a point where the selection is not set or is on an inactive object")]
        public required Selectable Element;

    void Update()
    {
        if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.activeInHierarchy == false)
            Element.Select();
    }
}
