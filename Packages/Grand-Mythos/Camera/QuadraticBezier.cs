using System;
using System.Collections.Generic;
using UnityEngine;
using Math = System.MathF;

/// <summary>
/// Partially lifted from https://blog.gludion.com/2009/08/distance-to-quadratic-bezier-curve.html
/// </summary>
[Serializable]
public struct QuadraticBezier
{
    public Vector3 p0, p1, p2;

    public QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        this.p0 = p0;
        this.p1 = p1;
        this.p2 = p2;
    }

    public static QuadraticBezier[] AutoTangents(List<Vector3> points, float f = 0.75f)
    {
        int n = points.Count;
        if (n < 2)
            throw new ArgumentException($"{nameof(points)} must contain at least two points");

        var beziers = new QuadraticBezier[n-1];

        var q = (points[0] + points[1]) / 2;
        beziers[0] = new QuadraticBezier(points[0], q, points[1]);
        for (int i = 1; i < n - 1; i++)
        {
            q = points[i] + (points[i] - q) * f;
            beziers[i] = new QuadraticBezier(points[i], q, points[i + 1]);
        }
        
        // Now we do it from end to start and take the midpoint between the two, this provides a more stable tangent
        q = (points[^1] + points[^2]) / 2;
        beziers[^1].p1 = (beziers[^1].p1 + q) * 0.5f;
        for (int i = 1; i < n - 1; i++)
        {
            q = points[^(i+1)] - (q - points[^(i+1)]) * f;
            beziers[^(i+1)].p1 = (beziers[^(i+1)].p1 + q) * 0.5f;
        }

        return beziers;
    }

    public Vector3 Sample(float t)
    {
        float oneMinusT = 1 - t;
        float a = oneMinusT * oneMinusT;
        float b = 2 * t * oneMinusT;
        float c = t * t;
        return a * p0 + b * p1 + c * p2;
    }

    public void GetBoundingBox(in PrecomputedData precomputedData, out Vector3 min, out Vector3 max)
    {
        // rough evaluation of bounds:
        min = Vector3.Min(p0, Vector3.Min(p1, p2));
        max = Vector3.Max(p0, Vector3.Min(p1, p2));

        // more accurate evaluation:
        // see Andree Michelle for a faster but less readable method
        for (int i = 0; i < 3; i++)
        {
            if (min[i] == p1[i] || max[i] == p1[i])
            {
                float u = -precomputedData.A[i] / precomputedData.B[i]; // u where getTan(u)[i] == 0
                float oneMinusU = 1 - u;
                u = oneMinusU * oneMinusU * p0[i] + 2 * u * oneMinusU * p1[i] + u * u * p2[i];
                if (min[i] == p1[i]) min[i] = u;
                else max[i] = u;
            }
        }
    }

    public float Length(PrecomputedData precomputedData)
    {
        var a = precomputedData.B;
        var b = 2 * precomputedData.A;


        // Compute dot products and norms
        float A = a.sqrMagnitude;
        float B = Vector3.Dot(a, b);
        float C = b.sqrMagnitude;

        if (A == 0f)
        {
            // Curve is a straight line
            return Mathf.Sqrt(C);
        }

        float Sabc = 2 * Mathf.Sqrt(A * C + B * B);
        float A_2 = Mathf.Sqrt(A);
        float A32 = 2 * A * A_2;
        float C2 = 2 * Mathf.Sqrt(C);
        float BA = B / A_2;

        // Final length using the closed-form expression
        return (A32 * Sabc + A_2 * B * (Sabc - C2) +
                (4 * C * A - B * B) * Mathf.Log((2 * A_2 + BA + Sabc) / (BA + C2))) / (4 * A32);
    }

    /// <summary>
    /// Finds the nearest point using a closed-form solution
    /// </summary>
    public void FindNearestPoint(in PrecomputedData cache, Vector3 test, out Vector3 posMin, out float tMin, out float distMin)
    {
        var deltaFrom0 = p0 - test;

        // search points P of Bézier curve with PM.(dP / dt) = 0
        // a calculus leads to a 3d degree equation :

        // Extrapolated the one below from this 2d one
        /*float a = cache.B.x * cache.B.x + cache.B.y * cache.B.y;
        float b = 3 * (cache.A.x * cache.B.x + cache.A.y * cache.B.y);
        float c = 2 * (cache.A.x * cache.A.x + cache.A.y * cache.A.y) + deltaFrom0.x * cache.B.x + deltaFrom0.y * cache.B.y;
        float d = deltaFrom0.x * cache.A.x + deltaFrom0.y * cache.A.y;*/

        float a = cache.B.sqrMagnitude;
        float b = 3 * Vector3.Dot(cache.A, cache.B);
        float c = 2 * cache.A.sqrMagnitude + Vector3.Dot(deltaFrom0, cache.B);
        float d = Vector3.Dot(deltaFrom0, cache.A);

        Span<float> solutions = stackalloc float[3];
        ThirdDegreeEquation(a, b, c, d, ref solutions);

        float d0 = deltaFrom0.sqrMagnitude;
        float d2 = (test - p2).sqrMagnitude;

        if (d0 < d2)
        {
            distMin = d0;
            tMin = 0;
            posMin = p0;
        }
        else
        {
            distMin = d2;
            tMin = 1;
            posMin = p2;
        }

        // Then test the solutions if they are closer
        foreach (float t in solutions)
        {
            if (t is < 0 or > 1)
                continue;

            var solutionSample = Sample(t);
            float dist = (test - solutionSample).sqrMagnitude;
            if (dist < distMin)
            {
                tMin = t;
                distMin = dist;
                posMin = solutionSample;
            }
        }

        return;

        static void ThirdDegreeEquation(float a, float b, float c, float d, ref Span<float> solutions)
        {
            if (Math.Abs(a) > float.Epsilon)
            {
                // let's adopt form: x3 + ax2 + bx + d = 0
                float z = a; // multi-purpose util variable
                a = b / z;
                b = c / z;
                c = d / z;
                // we solve using Cardan formula: http://fr.wikipedia.org/wiki/M%C3%A9thode_de_Cardan
                float p = b - a * a / 3;
                float q = a * (2 * a * a - 9 * b) / 27 + c;
                float p3 = p * p * p;
                float D = q * q + 4 * p3 / 27;
                float offset = -a / 3;
                if (D > float.Epsilon)
                {
                    // D positive
                    z = Math.Sqrt(D);
                    float u = (-q + z) / 2;
                    float v = (-q - z) / 2;
                    u = u >= 0 ? Math.Pow(u, 1 / 3f) : -Math.Pow(-u, 1f / 3);
                    v = v >= 0 ? Math.Pow(v, 1 / 3f) : -Math.Pow(-v, 1f / 3);
                    solutions = solutions[..1];
                    solutions[0] = u + v + offset;
                }
                else if (D < -float.Epsilon)
                {
                    // D negative
                    float u = 2 * Math.Sqrt(-p / 3);
                    float v = Math.Acos(-Math.Sqrt(-27 / p3) * q / 2) / 3;
                    solutions = solutions[..3];
                    solutions[0] = u * Math.Cos(v) + offset;
                    solutions[1] = u * Math.Cos(v + 2 * Math.PI / 3) + offset;
                    solutions[2] = u * Math.Cos(v + 4 * Math.PI / 3) + offset;
                }
                else
                {
                    // D zero
                    float u;
                    if (q < 0) u = Math.Pow(-q / 2, 1 / 3f);
                    else u = -Math.Pow(q / 2, 1 / 3f);
                    solutions = solutions[..2];
                    solutions[0] = 2 * u + offset;
                    solutions[1] = -u + offset;
                }
                return;
            }
            else // a is almost 0
            {
                // then actually a 2nd degree equation:
                // form : ax2 + bx + c = 0;
                a = b;
                b = c;
                c = d;
                if (Math.Abs(a) <= float.Epsilon)
                {
                    if (Math.Abs(b) <= float.Epsilon)
                    {
                        solutions = solutions[..0];
                        return;
                    }
                    else
                    {
                        solutions = solutions[..1];
                        solutions[0] = -c / b;
                        return;
                    }
                }

                float D = b * b - 4 * a * c;
                if (D <= -float.Epsilon)
                {
                    solutions = solutions[..0];
                    return;
                }

                if (D > float.Epsilon) // D positive
                {
                    D = Math.Sqrt(D);
                    solutions = solutions[..2];
                    solutions[0] = (-b - D) / (2 * a);
                    solutions[1] = (-b + D) / (2 * a);
                    return;
                }
                else if (D < -float.Epsilon) // D negative
                {
                    solutions = solutions[..0];
                    return;
                }
                else // D zero
                {
                    solutions = solutions[..1];
                    solutions[0] = -b / (2 * a);
                    return;
                }
            }
        }
    }

    public struct PrecomputedData
    {
        public readonly Vector3 A, B;

        public PrecomputedData(in QuadraticBezier curve)
        {
            A = curve.p1 - curve.p0;
            B = curve.p0 - 2 * curve.p1 + curve.p2;
        }

        // (dP/dt)(t) = 2*(A + t*B)
        public float GetBezierSpeed(float t)
        {
            var tan = A + t * B;
            return 2 * tan.magnitude;
        }

        public Vector3 GetBezierDirection(float t) => Vector3.Normalize(A + t * B);
    }
}