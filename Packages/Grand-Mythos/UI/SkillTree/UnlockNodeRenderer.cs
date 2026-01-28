using System;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[SelectionBase]
[RequireComponent(typeof(UnlockNode))]
public class UnlockNodeRenderer : Image
{
    const float LineThickness = 5;

    public Color LineColor = Color.white;

    public required UnlockNode Data;

    (Vector3, Vector3)[] _posCache = Array.Empty<(Vector3, Vector3)>();

    void Update()
    {
        Data ??= GetComponent<UnlockNode>();

        if (Data.LinkedTo.Length != _posCache.Length)
            RebuildLineRenderers();
        for (int i = 0; i < Data.LinkedTo.Length; i++)
        {
            if (_posCache[i] != (Data.LinkedTo[i].transform.position, this.transform.position))
            {
                RebuildLineRenderers();
                break;
            }
        }
    }

    protected override void OnPopulateMesh(VertexHelper vbo)
    {
        vbo.Clear();
        //base.OnPopulateMesh(vbo); // If we want to use the base mesh, which we don't as it is pretty useless sharing the sprite between the two ...

        var matrix = transform.worldToLocalMatrix;
        Span<Vector2> points = stackalloc Vector2[2];

        foreach (var requirement in Data.LinkedTo)
        {
            points[0] = matrix.MultiplyPoint3x4(transform.position);
            points[1] = matrix.MultiplyPoint3x4(requirement.transform.position);
            PointOnEdge(((RectTransform)transform), (RectTransform)requirement.transform, ref points[0], ref points[1]);

            for (int i = 1; i < points.Length; i++)
            {
                var prev = points[i - 1];
                var cur = points[i];

                var perpPos = Vector2.Perpendicular(cur - prev).normalized * LineThickness / 2f;
                var perpNeg = -perpPos;

                var v1 = prev + perpNeg;
                var v2 = prev + perpPos;
                var v3 = cur + perpPos;
                var v4 = cur + perpNeg;

                AddVert(vbo, v1);
                AddVert(vbo, v2);
                if (i != 1)
                    LastFourToTriangles(vbo); // Create a quad between the end of the previous line and the start of the current to remove any discontinuities
                AddVert(vbo, v3);
                AddVert(vbo, v4);
                LastFourToTriangles(vbo);
            }
        }
    }

    static void ClosestEdgeCenter(RectTransform a, RectTransform b, ref Vector2 aPoint, ref Vector2 bPoint)
    {
        var thisRect = a.rect;
        var otherRect = b.rect;
        var direction = aPoint - bPoint;
        int dim = Mathf.Abs(direction.x) > Mathf.Abs(direction.y) ? 0 : 1;
        aPoint[dim] += -Mathf.Sign(direction[dim]) * thisRect.size[dim] * 0.5f;
        bPoint[dim] += Mathf.Sign(direction[dim]) * otherRect.size[dim] * 0.5f;
    }

    static void PointOnEdge(RectTransform a, RectTransform b, ref Vector2 aPoint, ref Vector2 bPoint)
    {
        var rectA = a.rect;
        var rectB = b.rect;
        var direction = Vector3.Normalize(bPoint - aPoint);

        aPoint += IntersectBox(rectA, direction);
        bPoint += IntersectBox(rectB, -direction);

        static Vector2 IntersectBox(Rect rect, Vector3 dir)
        {
            var intersectRight = dir / dir.x * rect.width * 0.5f * Mathf.Sign(dir.x);
            var intersectUp = dir / dir.y * rect.height * 0.5f * Mathf.Sign(dir.y);
            if (dir.y == 0f)
                return intersectRight;
            else if (dir.x == 0f)
                return intersectUp;
            else
                return intersectUp.sqrMagnitude > intersectRight.sqrMagnitude && dir.y != 0 ? intersectRight : intersectUp;
        }
    }

    void AddVert(VertexHelper vbo, Vector2 vertex)
    {
        var vert = UIVertex.simpleVert;
        vert.color = LineColor;
        vert.position = vertex;
        vert.position.z = 1f;
        vbo.AddVert(vert);
    }

    void LastFourToTriangles(VertexHelper vbo)
    {
        vbo.AddTriangle(vbo.currentVertCount-3, vbo.currentVertCount-2, vbo.currentVertCount-1);
        vbo.AddTriangle(vbo.currentVertCount-1, vbo.currentVertCount-4, vbo.currentVertCount-3);
    }

    void RebuildLineRenderers()
    {
        _posCache = new (Vector3, Vector3)[Data.LinkedTo.Length];
        for (int i = 0; i < Data.LinkedTo.Length; i++)
            _posCache[i] = (Data.LinkedTo[i].transform.position, transform.position);

        SetVerticesDirty();
    }
}