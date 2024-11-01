using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ListScrollHelper : MonoBehaviour
{
    [Required] public InputActionReference ScrollInput;

    void Update()
    {
        float input = Mathf.Sign(ScrollInput.action.ReadValue<float>());
        if (ScrollInput.action.WasPerformedThisFrameUnique() && EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.GetComponentInParent<ScrollRect>() is {} rect)
        {
            float length = rect.verticalScrollbar.handleRect.rect.size.y;
            float total = ((RectTransform)rect.verticalScrollbar.transform).rect.size.y;
            if (length >= total)
                rect.verticalNormalizedPosition = input > 0 ? 1f : 0f;
            else
                rect.verticalNormalizedPosition += input * (length / (total - length)); // Scroll for one viewport
        }
    }
}