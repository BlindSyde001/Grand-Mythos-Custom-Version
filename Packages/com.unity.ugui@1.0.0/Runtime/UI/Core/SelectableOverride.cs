using System;
using JetBrains.Annotations;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    public partial class Selectable
    {
        enum Axis : int
        {
            X = 0, Y = 1
        }

        // Convenience function -- change the selection to the specified object if it's not null and happens to be active.
        void Navigate(AxisEventData eventData, Selectable sel)
        {
            if (sel != null && sel.IsActive())
                eventData.selectedObject = sel.gameObject;

            if (sel != null && sel.GetComponentInParent<ScrollRect>() is {} scrollRect)
            {
                for (var p = sel.transform; p != null; p = p.parent)
                {
                    if (p == scrollRect.content)
                    {
                        ScrollInView((RectTransform)sel.transform, scrollRect);
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

        /// <summary>
        /// <paramref name="dir3d"/> is treated as a 2d direction in canvas space, this method only operates in cardinal directions, diagonals are ignored
        /// </summary>
        [CanBeNull] public unsafe Selectable FindSelectable(Vector3 dir3d)
        {
            Axis axis = Mathf.Abs(dir3d.x) >= Mathf.Abs(dir3d.y) ? Axis.X : Axis.Y;
            int sign = dir3d[(int)axis] >= 0 ? 1 : -1;

            dir3d = default;
            dir3d[(int)axis] = sign;

            Vector2 dir = default;
            dir[(int)axis] = sign;

            var thisRectTransform = (RectTransform)transform;

            var thisRect = default(Rect);
            thisRect.min = thisRectTransform.TransformPoint(thisRectTransform.rect.min);
            thisRect.max = thisRectTransform.TransformPoint(thisRectTransform.rect.max);

            bool wantsWrapAround = navigation.wrapAround && m_Navigation.mode is Navigation.Mode.Vertical or Navigation.Mode.Horizontal;

            Span<(int, Rect)> allTargets = stackalloc (int, Rect)[s_SelectableCount];
            int effectiveTargets = 0;

            for (int i = 0; i < s_SelectableCount; ++i)
            {
                Selectable selectable = s_Selectables[i];

                if (selectable == this)
                    continue;

                if (!selectable.IsInteractable() || selectable.navigation.mode == Navigation.Mode.None)
                    continue;

#if UNITY_EDITOR
                // Apart from runtime use, FindSelectable is used by custom editors to
                // draw arrows between different selectables. For scene view cameras,
                // only selectables in the same stage should be considered.
                if (Camera.current != null && !UnityEditor.SceneManagement.StageUtility.IsGameObjectRenderedByCamera(selectable.gameObject, Camera.current))
                    continue;
#endif

                var target = (RectTransform)selectable.transform;
                var targetLocalRect = target.rect;

                var targetRect = targetLocalRect;
                targetRect.min = target.TransformPoint(targetLocalRect.min);
                targetRect.max = target.TransformPoint(targetLocalRect.max);

                allTargets[effectiveTargets++] = (i, targetRect);
            }

            var activeTargets = allTargets[..effectiveTargets];

            Vector2 thisEdge = thisRect.center/* + thisRect.size[(int)axis] * 0.5f * sign*/; // Actually, just take the center, that way intersecting rects are still handled efficiently
            (float major, float minor) closestDist = (float.PositiveInfinity, float.PositiveInfinity);
            float furthestDist = float.NegativeInfinity;
            Selectable closestSelectable = null;
            Selectable furthestSelectable = null;
            // First, sweep one of our rect's edge in the given direction, and retrieve the closest rect that is in the way
            foreach (var (selectable, targetRect) in activeTargets)
            {
                float overlapTop, overlapBot;
                if (axis == (int)Axis.X)
                {
                    overlapTop = Mathf.Min(targetRect.max.y, thisRect.max.y);
                    overlapBot = Mathf.Max(targetRect.min.y, thisRect.min.y);
                }
                else
                {
                    overlapTop = Mathf.Min(targetRect.max.x, thisRect.max.x);
                    overlapBot = Mathf.Max(targetRect.min.x, thisRect.min.x);
                }

                if (overlapBot >= overlapTop)
                    continue; // Sweeping our edge in that direction did not hit the target

                Vector2 targetEdge = targetRect.center - targetRect.size[(int)axis] * 0.5f * dir;
                Vector2 delta = targetEdge - thisEdge;
                float distanceMajor = Mathf.Abs(delta[(int)axis]);
                if (Math.Sign(delta[(int)axis]) != sign) // target is backwards compared to our direction
                {
                    if (distanceMajor > furthestDist)
                    {
                        furthestSelectable = s_Selectables[selectable];
                        furthestDist = distanceMajor;
                    }
                }
                else
                {
                    float distanceMinor = Mathf.Abs(delta[1 - (int)axis]);
                    // If distance on the direction axis are the same, pick the one that has the list difference in the other axis ; i.e.: if same distance on X, see if the Y is closest
                    if (distanceMajor < closestDist.major || Mathf.Approximately(distanceMajor, closestDist.major) && distanceMinor < closestDist.minor)
                    {
                        closestSelectable = s_Selectables[selectable];
                        closestDist = (distanceMajor, distanceMinor);
                    }
                }
            }

            if (closestSelectable != null)
                return closestSelectable;

            if (wantsWrapAround && furthestSelectable != null)
                return furthestSelectable;

            // If that failed, pick the closest rect to us which is closest to the edge

            var pointOnEdge = thisRect.center + thisRect.size[(int)axis] * 0.5f * dir;
            closestDist = (float.PositiveInfinity, float.PositiveInfinity);
            furthestDist = float.NegativeInfinity;
            closestSelectable = null;
            furthestSelectable = null;
            foreach (var (selectable, targetRect) in activeTargets)
            {
                var pointOnTarget = targetRect.center;

                var direction = Vector3.Normalize(pointOnTarget - pointOnEdge);

                pointOnTarget += IntersectBox(targetRect, -direction);

                Vector2 delta = pointOnTarget - pointOnEdge;
                if (Mathf.Abs(delta[(int)axis]) <= float.Epsilon)
                    continue;

                float deltaInDir = delta[(int)axis];
                float distance1D = Mathf.Abs(deltaInDir);
                float distance2D = delta.sqrMagnitude;
                if (Math.Sign(deltaInDir) != sign) // target is backwards compared to our direction
                {
                    if (distance1D > furthestDist) // Take the furthest one on the same axis
                    {
                        furthestSelectable = s_Selectables[selectable];
                        furthestDist = distance1D;
                    }
                }
                else
                {
                    if (distance2D < closestDist.major) // Take the closest one in both axes
                    {
                        closestSelectable = s_Selectables[selectable];
                        closestDist = (distance2D, float.PositiveInfinity);
                    }
                }
            }

            if (closestSelectable != null)
                return closestSelectable;

            if (wantsWrapAround && furthestSelectable != null)
                return furthestSelectable;

            return null;

            static Vector2 IntersectBox(Rect rect, Vector3 dir1)
            {
                var intersectRight = dir1 / (dir1.x * rect.width * 0.5f * Mathf.Sign(dir1.x));
                var intersectUp = dir1 / (dir1.y * rect.height * 0.5f * Mathf.Sign(dir1.y));
                if (dir1.y == 0f)
                    return intersectRight;
                else if (dir1.x == 0f)
                    return intersectUp;
                else
                    return intersectUp.sqrMagnitude > intersectRight.sqrMagnitude && dir1.y != 0 ? intersectRight : intersectUp;
            }
        }
    }
}