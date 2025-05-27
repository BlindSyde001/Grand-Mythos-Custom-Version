using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sirenix.OdinInspector;
using UnityEngine;

public class PointOfViewSpline2 : PointOfViewBase
{
    [OnValueChanged(nameof(RebuildBezier))]
    public List<Vector3> Points = new(){ new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 0, 2) };
    public Vector3 Offset = new(0.5f, 0.5f, 0f);
    [OnValueChanged(nameof(RebuildBezier)), Range(0,1)] 
    public float F = 1f;
    [ReadOnly] public QuadraticBezier[] Beziers = Array.Empty<QuadraticBezier>();


    private Matrix4x4 LocalMatrix => Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
    
    public override void ComputeWorldTransform(Vector3 worldPosFocus, out Vector3 position, out Quaternion rotation)
    {
        var localFocus = Quaternion.Inverse(transform.rotation) * (worldPosFocus - transform.position);
        
        (Vector3 pMin, float tMin, float distMin, int index) closest = (default, -1f, float.PositiveInfinity, -1);
        for (var i = 0; i < Beziers.Length; i++)
        {
            var bezier = Beziers[i];
            var d = new QuadraticBezier.PrecomputedData(bezier);
            bezier.FindNearestPoint(d, localFocus, out var pMin, out var tMin, out var distMin);
            if (closest.distMin > distMin)
                closest = (pMin, tMin, distMin, i);
        }

        Vector3 bezierDirection; 
        {
            var i = closest.index;
            var bezier = Beziers[i];
            var precomp = new QuadraticBezier.PrecomputedData(bezier);
            bezierDirection = precomp.GetBezierDirection(closest.tMin);
        }

        // Transform to world space
        position = transform.position + transform.rotation * closest.pMin + Quaternion.LookRotation(bezierDirection) * Offset;
        rotation = Quaternion.LookRotation((worldPosFocus - position).normalized);
    }

    private void RebuildBezier()
    {
        Beziers = QuadraticBezier.AutoTangents(Points, F);
    }


#if UNITY_EDITOR
    private static Stopwatch editor_sw = Stopwatch.StartNew();

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        var initialMatrix = Gizmos.matrix;

        { // Frustum
            Gizmos.color = Color.cyan;
            var i = (int)(editor_sw.Elapsed.TotalSeconds % Beziers.Length);
            var bezier = Beziers[i];
            var precomp = new QuadraticBezier.PrecomputedData(bezier);

            var t = (float)(editor_sw.Elapsed.TotalSeconds % 1f);
            var sample = bezier.Sample(t);
            var dir = precomp.GetBezierDirection(t);

            var position = transform.position + transform.rotation * sample + Quaternion.LookRotation(dir) * Offset;
            var rotation = Quaternion.LookRotation((transform.position + transform.rotation * sample - position).normalized);
            Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
            DrawCameraFrustum();
        }
        Gizmos.matrix = initialMatrix;
    }

    protected override void DuringSceneGui(UnityEditor.SceneView obj)
    {
        const int amountOfSegments = 20;

        base.DuringSceneGui(obj);

        var matrix = UnityEditor.Handles.matrix;
        UnityEditor.Handles.matrix = LocalMatrix;

        { // Curve
            Gizmos.color = Color.grey;
            Gizmos.matrix = LocalMatrix;
            foreach (var bezier in Beziers)
            {
                var pBase = bezier.p0;
                for (int j = 1; j <= amountOfSegments; j++)
                {
                    var t = (float)j / amountOfSegments;
                    var point = bezier.Sample(t);
                    UnityEditor.Handles.DrawLine(pBase, point);
                    pBase = point;
                }
            }
        }

        UnityEditor.EditorGUI.BeginChangeCheck();
        for (int i = 0; i < Points.Count; i++)
        {
            Points[i] = UnityEditor.Handles.PositionHandle(Points[i], Quaternion.identity);
        }

        Offset = UnityEditor.Handles.PositionHandle(Offset, Quaternion.identity);
        if (UnityEditor.EditorGUI.EndChangeCheck())
        {
            RebuildBezier();
        }

        UnityEditor.Handles.matrix = matrix;
    }
    #endif
}