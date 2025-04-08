using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PointOfViewSpline : PointOfViewBase
{
    [Range(0,1)] public float Alpha = 0.5f;
    [Range(0,1)] public float InvTension = 1f;
    public List<Vector3> Points = new(){ new Vector3(0, 1, 0), new Vector3(0, 1, 1) };
    public Quaternion FixedRotation = Quaternion.Euler(45, 0, 0);


    private Matrix4x4 LocalMatrix => Matrix4x4.TRS(transform.position + Vector3.one * 0.1f /* slightly offset because adding points is on 0,0,0 which overlaps the transform gizmo */, transform.rotation, Vector3.one);
    
    public override void ComputeWorldTransform(Vector3 worldPosFocus, out Vector3 position, out Quaternion rotation)
    {
        var thisCenter = transform.position + Vector3.one * 0.1f;

        // This is in local space
        GetSplinePlane(out var planePoint, out var planeNormal);

        var localFocus = Quaternion.Inverse(transform.rotation) * (worldPosFocus - thisCenter);

        // Move local focus to a point on the spline's plane where it would be centered given the FixedRotation
        GetPOVOffsetDirection(out var cameraOffsetDirection);
        var plane = new Plane(planeNormal, planePoint);
        plane.Raycast(new Ray(localFocus, cameraOffsetDirection), out float enter);
        localFocus += cameraOffsetDirection * enter; // do note that enter is more likely to be negative here, but we aren't changing it for performance purposes
        
        GetClosestPoint(localFocus, 30, out position, out rotation);

        // Transform to world space
        position = transform.rotation * position + thisCenter;
        rotation = transform.rotation * rotation;
    }

    public void GetSplinePlane(out Vector3 point, out Vector3 normal)
    {
        point = Points[0];
        var splineDir = Points[^1] - point;
        normal = Quaternion.LookRotation(splineDir.normalized) * FixedRotation * Vector3.forward;
        Vector3.OrthoNormalize(ref splineDir, ref normal);
    }

    public void GetPOVOffsetDirection(out Vector3 direction)
    {
        direction = Quaternion.LookRotation((Points[^1] - Points[0]).normalized) * FixedRotation * new Vector3(0,0,-1);
    }

    public void PrecomputeCurveSegment(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, out PrecomputedCurve curve)
    {
        float t01 = Mathf.Pow(Vector3.Distance(p0, p1), Alpha);
        float t12 = Mathf.Pow(Vector3.Distance(p1, p2), Alpha);
        float t23 = Mathf.Pow(Vector3.Distance(p2, p3), Alpha);

        var m1 = InvTension * (p2 - p1 + t12 * ((p1 - p0) / t01 - (p2 - p0) / (t01 + t12)));
        var m2 = InvTension * (p2 - p1 + t12 * ((p3 - p2) / t23 - (p3 - p1) / (t12 + t23)));

        curve.A = 2f * (p1 - p2) + m1 + m2;
        curve.B = -3f * (p1 - p2) - m1 - m1 - m2;
        curve.C = m1;
        curve.D = p1;
    }

    public void PrecomputeCurveFromIndex(int i, out PrecomputedCurve curve)
    {
        Vector3 pA = i < 1 ? Points[0] + (Points[0] - Points[1]) : Points[i-1];
        Vector3 pB = Points[i];
        Vector3 pC = i == Points.Count - 1 ? Points[i] + (Points[^1] - Points[^2]).normalized : Points[i+1];
        Vector3 pD = i >= Points.Count - 2 ? pC + (Points[^1] - Points[^2]).normalized : Points[i+2];
        
        PrecomputeCurveSegment(pA, pB, pC, pD, out curve);
    }

    public void GetClosestIndex(Vector3 localPosition, out int index)
    {
        (int index, float distance) closest = (0, float.PositiveInfinity);

        for (int i = 1; i < Points.Count; i++)
        {
            var closestPoint = FindClosestPointOnLine(Points[i-1], Points[i], localPosition);
            var distance = (closestPoint - localPosition).sqrMagnitude;
            if (distance < closest.distance)
                closest = (i-1, distance);
        }

        index = closest.index;

        
        static Vector3 FindClosestPointOnLine(Vector3 origin, Vector3 end, Vector3 point)
        {
            //Get heading
            Vector3 heading = end - origin;
            float magnitudeMax = heading.magnitude;
            heading.Normalize();

            //Do projection from the point but clamp it
            Vector3 lhs = point - origin;
            float dotP = Vector3.Dot(lhs, heading);
            dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
            return origin + heading * dotP;
        }
    }

    public void GetClosestPointOnCurveSegment(Vector3 localPoint, PrecomputedCurve curve, int maximumAmountOfSteps, out float closestT)
    {
        const float ratioMin = 0.25f;
        const float ratioMax = 1f-ratioMin;
        
        float tA = 0f;
        float tB = 1f;

        float sideADist = (localPoint - SampleCurve(curve, tA)).sqrMagnitude;
        float sideBDist = (localPoint - SampleCurve(curve, tB)).sqrMagnitude;
        do
        {
            if (sideADist > sideBDist)
            {
                tA = tA * ratioMax + tB * ratioMin;
                sideADist = (localPoint - SampleCurve(curve, tA)).sqrMagnitude;
            }
            else
            {
                tB = tA * ratioMin + tB * ratioMax;
                sideBDist = (localPoint - SampleCurve(curve, tB)).sqrMagnitude;
            }
        } while (maximumAmountOfSteps-- > 0);
        
        closestT = tA * 0.5f + tB * 0.5f;
    }

    public void GetClosestPoint(Vector3 testLocal, int maximumAmountOfSteps, out Vector3 posLocal, out Quaternion rotLocal)
    {
        GetClosestIndex(testLocal, out var index);
        PrecomputeCurveFromIndex(index, out var curve);
        GetClosestPointOnCurveSegment(testLocal, curve, maximumAmountOfSteps, out float closestT);
        SampleCurve(curve, closestT, out posLocal, out rotLocal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector3 SampleCurve(PrecomputedCurve curve, float t)
    {
        return curve.A * t * t * t +
               curve.B * t * t +
               curve.C * t +
               curve.D;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SampleCurve(PrecomputedCurve curve, float t, out Vector3 position, out Quaternion rotation)
    {
        position = SampleCurve(curve, t);
        var dir = Vector3.Normalize(SampleCurve(curve, t+0.05f) - position);
        rotation = Quaternion.LookRotation(dir) * FixedRotation;
    }

#if UNITY_EDITOR
    private static Stopwatch editor_sw = Stopwatch.StartNew();

    protected override void OnDrawGizmosSelected()
    {
        const int amountOfSegments = 20;

        base.OnDrawGizmosSelected();
        
        if (Points.Count < 2)
            return;

        var initialMatrix = Gizmos.matrix;

        { // Curve
            Gizmos.color = Color.grey;
            Gizmos.matrix = LocalMatrix;
            for (int i = 0; i < Points.Count; i++)
            {
                PrecomputeCurveFromIndex(i, out var curve);

                var pBase = curve.D;
                for (int j = 1; j <= amountOfSegments; j++)
                {
                    var t = (float)j / amountOfSegments;
                    var point = SampleCurve(curve, t);
                    Gizmos.DrawLine(pBase, point);
                    pBase = point;
                }
            }
        }

        { // Plane
            GetClosestPoint(default, 10, out var localPos, out var localRot);
            GetSplinePlane(out var planePoint, out var planeNormal);
            Gizmos.matrix = LocalMatrix * Matrix4x4.TRS(localPos, Quaternion.LookRotation(planeNormal), Vector3.one);
            Gizmos.DrawWireCube(default, new Vector3(1, 1, 0));
        }

        { // Frustum
            Gizmos.color = Color.cyan;
            PrecomputeCurveFromIndex((int)(editor_sw.Elapsed.TotalSeconds % (Points.Count-1)), out var curve);
            SampleCurve(curve, (float)(editor_sw.Elapsed.TotalSeconds % 1f), out var pos, out var rot);

            Gizmos.matrix = LocalMatrix * Matrix4x4.TRS(pos, rot.normalized, Vector3.one);
            DrawCameraFrustum();
        }
        Gizmos.matrix = initialMatrix;
    }

    protected override void DuringSceneGui(UnityEditor.SceneView obj)
    {
        base.DuringSceneGui(obj);

        var matrix = UnityEditor.Handles.matrix;
        UnityEditor.Handles.matrix = LocalMatrix;

        for (int i = 0; i < Points.Count; i++)
        {
            Points[i] = UnityEditor.Handles.PositionHandle(Points[i], Quaternion.identity);
        }

        GetClosestPoint(new Vector3(0, 0, 0), 10, out var pos, out var rot);
        FixedRotation = UnityEditor.Handles.RotationHandle(FixedRotation, pos);

        UnityEditor.Handles.matrix = matrix;
    }
    #endif

    public ref struct PrecomputedCurve
    {
        public Vector3 A, B, C, D;
    }
}