using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu(" GrandMythos/UI/DefaultToFirstSelectable")]
public class DefaultToFirstSelectable : MonoBehaviour
{
    void Update()
    {
        if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.activeInHierarchy == false)
            GetComponentInChildren<Selectable>().Select();
    }
}
