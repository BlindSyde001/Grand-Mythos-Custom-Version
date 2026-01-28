using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Layout class to arrange child elements in a grid format.
/// </summary>
/// <remarks>
/// The GridLayoutGroup component is used to layout child layout elements in a uniform grid where all cells have the same size. The size and the spacing between cells is controlled by the GridLayoutGroup itself. The children have no influence on their sizes.
/// </remarks>
[AddComponentMenu("Layout/Slanted Grid Layout Group", 152)]
public class SlantedGridLayoutGroup : GridLayoutGroup
{
    [SerializeField] protected float m_Slantedness = 2;

    /// <summary>
    /// How many cells there should be along the constrained axis.
    /// </summary>
    public float slantedness { get { return m_Slantedness; } set { SetProperty(ref m_Slantedness, Mathf.Max(0, value)); } }

    protected SlantedGridLayoutGroup()
    {}


    /// <summary>
    /// Called by the layout system
    /// Also see ILayoutElement
    /// </summary>
    public override void SetLayoutHorizontal()
    {
        SetCellsAlongAxis(0);
    }

    /// <summary>
    /// Called by the layout system
    /// Also see ILayoutElement
    /// </summary>
    public override void SetLayoutVertical()
    {
        SetCellsAlongAxis(1);
    }

    private void SetCellsAlongAxis(int axis)
    {
        // Normally a Layout Controller should only set horizontal values when invoked for the horizontal axis
        // and only vertical values when invoked for the vertical axis.
        // However, in this case we set both the horizontal and vertical position when invoked for the vertical axis.
        // Since we only set the horizontal position and not the size, it shouldn't affect children's layout,
        // and thus shouldn't break the rule that all horizontal layout must be calculated before all vertical layout.
        var rectChildrenCount = rectChildren.Count;
        if (axis == 0)
        {
            // Only set the sizes when invoked for horizontal axis, not the positions.

            for (int i = 0; i < rectChildrenCount; i++)
            {
                RectTransform rect = rectChildren[i];

                m_Tracker.Add(this, rect,
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.AnchoredPosition |
                    DrivenTransformProperties.SizeDelta);

                rect.anchorMin = Vector2.up;
                rect.anchorMax = Vector2.up;
                rect.sizeDelta = cellSize;
            }
            return;
        }

        float width = rectTransform.rect.size.x;
        float height = rectTransform.rect.size.y;

        int cellCountX = 1;
        int cellCountY = 1;
        if (m_Constraint == Constraint.FixedColumnCount)
        {
            cellCountX = m_ConstraintCount;

            if (rectChildrenCount > cellCountX)
                cellCountY = rectChildrenCount / cellCountX + (rectChildrenCount % cellCountX > 0 ? 1 : 0);
        }
        else if (m_Constraint == Constraint.FixedRowCount)
        {
            cellCountY = m_ConstraintCount;

            if (rectChildrenCount > cellCountY)
                cellCountX = rectChildrenCount / cellCountY + (rectChildrenCount % cellCountY > 0 ? 1 : 0);
        }
        else
        {
            if (cellSize.x + spacing.x <= 0)
                cellCountX = int.MaxValue;
            else
                cellCountX = Mathf.Max(1, Mathf.FloorToInt((width - padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));

            if (cellSize.y + spacing.y <= 0)
                cellCountY = int.MaxValue;
            else
                cellCountY = Mathf.Max(1, Mathf.FloorToInt((height - padding.vertical + spacing.y + 0.001f) / (cellSize.y + spacing.y)));
        }

        int cornerX = (int)startCorner % 2;
        int cornerY = (int)startCorner / 2;

        int cellsPerMainAxis, actualCellCountX, actualCellCountY;
        if (startAxis == Axis.Horizontal)
        {
            cellsPerMainAxis = cellCountX;
            actualCellCountX = Mathf.Clamp(cellCountX, 1, rectChildrenCount);

            if (m_Constraint == Constraint.FixedRowCount)
                actualCellCountY = Mathf.Min(cellCountY, rectChildrenCount);
            else
                actualCellCountY = Mathf.Clamp(cellCountY, 1, Mathf.CeilToInt(rectChildrenCount / (float)cellsPerMainAxis));
        }
        else
        {
            cellsPerMainAxis = cellCountY;
            actualCellCountY = Mathf.Clamp(cellCountY, 1, rectChildrenCount);

            if (m_Constraint == Constraint.FixedColumnCount)
                actualCellCountX = Mathf.Min(cellCountX, rectChildrenCount);
            else
                actualCellCountX = Mathf.Clamp(cellCountX, 1, Mathf.CeilToInt(rectChildrenCount / (float)cellsPerMainAxis));
        }

        Vector2 requiredSpace = new Vector2(
            actualCellCountX * cellSize.x + (actualCellCountX - 1) * spacing.x,
            actualCellCountY * cellSize.y + (actualCellCountY - 1) * spacing.y
        );
        Vector2 startOffset = new Vector2(
            GetStartOffset(0, requiredSpace.x),
            GetStartOffset(1, requiredSpace.y)
        );

        // Fixes case 1345471 - Makes sure the constraint column / row amount is always respected
        int childrenToMove = 0;
        if (rectChildrenCount > m_ConstraintCount && Mathf.CeilToInt((float)rectChildrenCount / (float)cellsPerMainAxis) < m_ConstraintCount)
        {
            childrenToMove = m_ConstraintCount - Mathf.CeilToInt((float)rectChildrenCount / (float)cellsPerMainAxis);
            childrenToMove += Mathf.FloorToInt((float)childrenToMove / ((float)cellsPerMainAxis - 1));
            if (rectChildrenCount % cellsPerMainAxis == 1)
                childrenToMove += 1;
        }

        for (int i = 0; i < rectChildrenCount; i++)
        {
            int positionX;
            int positionY;
            if (startAxis == Axis.Horizontal)
            {
                if (m_Constraint == Constraint.FixedRowCount && rectChildrenCount - i <= childrenToMove)
                {
                    positionX = 0;
                    positionY = m_ConstraintCount - (rectChildrenCount - i);
                }
                else
                {
                    positionX = i % cellsPerMainAxis;
                    positionY = i / cellsPerMainAxis;
                }
            }
            else
            {
                if (m_Constraint == Constraint.FixedColumnCount && rectChildrenCount - i <= childrenToMove)
                {
                    positionX = m_ConstraintCount - (rectChildrenCount - i);
                    positionY = 0;
                }
                else
                {
                    positionX = i / cellsPerMainAxis;
                    positionY = i % cellsPerMainAxis;
                }
            }

            if (cornerX == 1)
                positionX = actualCellCountX - 1 - positionX;
            if (cornerY == 1)
                positionY = actualCellCountY - 1 - positionY;

            SetChildAlongAxis(rectChildren[i], 0, startOffset.x + (cellSize[0] + spacing[0]) * positionX + m_Slantedness * positionY, cellSize[0]);
            SetChildAlongAxis(rectChildren[i], 1, startOffset.y + (cellSize[1] + spacing[1]) * positionY, cellSize[1]);
        }
    }
}
