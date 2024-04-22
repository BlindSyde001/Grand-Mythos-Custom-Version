using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AutoScrollListviewToSelection : MonoBehaviour
{
    void Update()
    {
        var selection = EventSystem.current.currentSelectedGameObject;
        if (selection != null && selection.GetComponentInParent<ScrollRect>() is {} scrollRect)
        {
            for (var p = selection.transform; p != null; p = p.parent)
            {
                if (p == scrollRect.content)
                {
                    ScrollInView((RectTransform)selection.transform, scrollRect);
                    break;
                }
            }
        }
    }

    void ScrollInView(RectTransform child, ScrollRect scrollRect)
    {
        // Source: https://stackoverflow.com/a/76485551
        var viewPosMin = scrollRect.viewport.rect.min;
        var viewPosMax = scrollRect.viewport.rect.max;

        var childPosMin = scrollRect.viewport.InverseTransformPoint(child.TransformPoint(child.rect.min));
        var childPosMax = scrollRect.viewport.InverseTransformPoint(child.TransformPoint(child.rect.max));

        var move = Vector2.zero;

        if (childPosMin.y < viewPosMin.y)
            move.y += childPosMin.y - viewPosMin.y;
        if (childPosMax.y > viewPosMax.y)
            move.y += childPosMax.y - viewPosMax.y;
        if (childPosMin.x < viewPosMin.x)
            move.x += childPosMin.x - viewPosMin.x;
        if (childPosMax.x > viewPosMax.x)
            move.x += childPosMax.x - viewPosMax.x;

        Vector3 worldMove = scrollRect.viewport.TransformDirection(move);
        scrollRect.content.localPosition -= scrollRect.content.InverseTransformDirection(worldMove);
    }
}