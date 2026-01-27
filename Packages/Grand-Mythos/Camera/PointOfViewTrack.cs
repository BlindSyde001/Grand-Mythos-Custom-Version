using System;
using System.Diagnostics;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class PointOfViewTrack : PointOfViewBase
{
    public AnimationCurve3D Curve = new();
    public Vector3 Start = new(0, -1f, -1), End = new(0, -1f, 1);

    private Matrix4x4 LocalMatrix => Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
    
    public override void ComputeWorldTransform(Vector3 worldPosFocus, out Vector3 position, out Quaternion rotation)
    {
        var localFocus = Quaternion.Inverse(transform.rotation) * (worldPosFocus - transform.position);
        var path = End - Start;
        var projection = Vector3.Project(localFocus - Start, path);
        float t;
        if (Vector3.Dot(path, projection) < 0)
            t = 0f;
        else
            t = projection.magnitude / path.magnitude;

        var sample = Curve.Sample(t);

        // Transform to world space
        position = transform.position + transform.rotation * sample.pos;
        rotation = transform.rotation * sample.rot;
    }


#if UNITY_EDITOR
    private static Stopwatch editor_sw = Stopwatch.StartNew();

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        var initialMatrix = Gizmos.matrix;

        { // Frustum
            Gizmos.color = Color.cyan;

            {
                var t = (float)(editor_sw.Elapsed.TotalSeconds * 0.25f % 1f);
                var wPos = transform.position + transform.rotation * Vector3.Lerp(Start, End, t);
                ComputeWorldTransform(wPos, out Vector3 position, out var rotation);
                Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
                DrawCameraFrustum();

                Gizmos.color = Color.black;
                Gizmos.matrix = initialMatrix;
                Gizmos.DrawLine(position, wPos);
            }

            for (int j = 0; j < Curve.Count; j++)
            {
                var (position, rotation) = Curve[j];
                position = transform.position + transform.rotation * position;
                rotation = transform.rotation * rotation;
                Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one*0.33f);
                Gizmos.color = Color.cyan;
                DrawCameraFrustum();
            }
        }
        Gizmos.matrix = initialMatrix;
    }

    protected override void DuringSceneGui(UnityEditor.SceneView obj)
    {
        const int amountOfSegments = 20;

        base.DuringSceneGui(obj);

        var matrix = UnityEditor.Handles.matrix;
        UnityEditor.Handles.matrix = LocalMatrix;
        var previousColor = UnityEditor.Handles.color;

        { // Curve
            UnityEditor.Handles.color = Color.cyan;
            Gizmos.matrix = LocalMatrix;
            var previous = Curve.Sample(0);
            for (int j = 1; j <= amountOfSegments; j++)
            {
                var t = (float)j / amountOfSegments;
                var pair = Curve.Sample(t);
                UnityEditor.Handles.DrawLine(previous.pos, pair.pos);
                previous = pair;
            }
        }

        UnityEditor.EditorGUI.BeginChangeCheck();
        UnityEditor.Undo.RecordObject(this, nameof(PointOfViewTrack));

        for (int i = 0; i < Curve.Count; i++)
        {
            Curve[i] = new(UnityEditor.Handles.PositionHandle(Curve[i].pos, Quaternion.identity), UnityEditor.Handles.RotationHandle(Curve[i].rot, Curve[i].pos));
        }

        for (var i = 0; i < Curve.Count; i++)
        {
            var val = Curve[i];

            var handleSize = UnityEditor.HandleUtility.GetHandleSize(val.pos) * 0.1f; 
            {
                UnityEditor.Handles.color = Color.red;
                if (UnityEditor.Handles.Button(val.pos + Vector3.down * 0.1f, val.rot, handleSize, handleSize, UnityEditor.Handles.SphereHandleCap))
                {
                    Curve.Remove(i);
                }
                UnityEditor.Handles.Label(val.pos, "Remove");
            }


            (Vector3 pos, Quaternion rot) middle;
            if (i == Curve.Count - 1)
            {
                float t = (i - 0.5f) / (Curve.Count - 1);
                middle = Curve.Sample(t);
                middle = (pos: Curve[^1].pos + (Curve[^1].pos - middle.pos), rot: Curve[^1].rot);
            }
            else
            {
                float t = (i + 0.5f) / (Curve.Count - 1);
                middle = Curve.Sample(t);
            }

            UnityEditor.Handles.color = Color.green;
            if (UnityEditor.Handles.Button(middle.pos, middle.rot, handleSize, handleSize, UnityEditor.Handles.SphereHandleCap))
            {
                Curve.Insert(i+1, middle.pos, middle.rot);
            }

            UnityEditor.Handles.Label(middle.pos, "Add");

            if (i == 0)
            {
                UnityEditor.Handles.color = Color.green;
                middle = (pos: Curve[0].pos + (Curve[0].pos - middle.pos), rot: Curve[0].rot);
                if (UnityEditor.Handles.Button(middle.pos, middle.rot, handleSize, handleSize, UnityEditor.Handles.SphereHandleCap))
                {
                    Curve.Insert(i, middle.pos, middle.rot);
                }

                UnityEditor.Handles.Label(middle.pos, "Add");
            }
        }

        Start = UnityEditor.Handles.PositionHandle(Start, Quaternion.identity);
        End = UnityEditor.Handles.PositionHandle(End, Quaternion.identity);

        UnityEditor.Handles.color = Color.green;

        UnityEditor.Handles.DrawLine(Start, End);
        
        UnityEditor.Handles.Label((Start+End)/2f, "Player Path");

        UnityEditor.Handles.color = previousColor;
        UnityEditor.Handles.matrix = matrix;
    }
    #endif
}

[Serializable]
public class AnimationCurve3D
{
    [InfoBox("If you add or remove a value, make sure to do so on other curves as well", InfoMessageType.Warning)]
    public AnimationCurve VX = AnimationCurve.Linear(0, 0, 1, 0);
    public AnimationCurve VY = AnimationCurve.Linear(0, 1, 1, 1);
    public AnimationCurve VZ = AnimationCurve.Linear(0, -1, 1, 1);

    public AnimationCurve QX = AnimationCurve.Linear(0, 0, 1, 0);
    public AnimationCurve QY = AnimationCurve.Linear(0, 0, 1, 0);
    public AnimationCurve QZ = AnimationCurve.Linear(0, 0, 1, 0);
    public AnimationCurve QW = AnimationCurve.Linear(0, 1, 1, 1);

    public int Count => VX.length;

    public (Vector3 pos, Quaternion rot) this[int index]
    {
        get => (new Vector3(VX[index].value, VY[index].value, VZ[index].value),
            new Quaternion(QX[index].value, QY[index].value, QZ[index].value, QW[index].value));
        set
        {
            if (this[index] == value)
                return;
            
            var vX = VX.keys;
            var vY = VY.keys;
            var vZ = VZ.keys;
            vX[index] = new(VX[index].time, value.pos.x);
            vY[index] = new(VY[index].time, value.pos.y);
            vZ[index] = new(VZ[index].time, value.pos.z);
            VX.keys = vX;
            VY.keys = vY;
            VZ.keys = vZ;
            
            var qX = QX.keys;
            var qY = QY.keys;
            var qZ = QZ.keys;
            var qW = QW.keys;
            qX[index] = new(QX[index].time, value.rot.x);
            qY[index] = new(QY[index].time, value.rot.y);
            qZ[index] = new(QZ[index].time, value.rot.z);
            qW[index] = new(QW[index].time, value.rot.w);
            QX.keys = qX;
            QY.keys = qY;
            QZ.keys = qZ;
            QW.keys = qW;
            Reprocess();
        }
    }

    public void Insert(int i, Vector3 value, Quaternion quaternion)
    {
        var previousCount = Count;
        var x = VX.keys.ToList();
        var y = VY.keys.ToList();
        var z = VZ.keys.ToList();
        x.Insert(i, new Keyframe(i == 0 ? 0 : i == previousCount ? 1.1f : x[i].time - float.Epsilon, value.x));
        y.Insert(i, new Keyframe(i == 0 ? 0 : i == previousCount ? 1.1f : y[i].time - float.Epsilon, value.y));
        z.Insert(i, new Keyframe(i == 0 ? 0 : i == previousCount ? 1.1f : z[i].time - float.Epsilon, value.z));
        VX.keys = x.ToArray();
        VY.keys = y.ToArray();
        VZ.keys = z.ToArray();
            
        var qX = QX.keys.ToList();
        var qY = QY.keys.ToList();
        var qZ = QZ.keys.ToList();
        var qW = QW.keys.ToList();
        qX.Insert(i, new Keyframe(i == 0 ? 0 : i == previousCount ? 1.1f : qX[i].time - float.Epsilon, quaternion.x));
        qY.Insert(i, new Keyframe(i == 0 ? 0 : i == previousCount ? 1.1f : qY[i].time - float.Epsilon, quaternion.y));
        qZ.Insert(i, new Keyframe(i == 0 ? 0 : i == previousCount ? 1.1f : qZ[i].time - float.Epsilon, quaternion.z));
        qW.Insert(i, new Keyframe(i == 0 ? 0 : i == previousCount ? 1.1f : qW[i].time - float.Epsilon, quaternion.w));
        QX.keys = qX.ToArray();
        QY.keys = qY.ToArray();
        QZ.keys = qZ.ToArray();
        QW.keys = qW.ToArray();

        Reprocess();
    }

    public void Remove(int i)
    {
        var x = VX.keys.ToList();
        var y = VY.keys.ToList();
        var z = VZ.keys.ToList();
        x.RemoveAt(i);
        y.RemoveAt(i);
        z.RemoveAt(i);
        VX.keys = x.ToArray();
        VY.keys = y.ToArray();
        VZ.keys = z.ToArray();
            
        var qX = QX.keys.ToList();
        var qY = QY.keys.ToList();
        var qZ = QZ.keys.ToList();
        var qW = QW.keys.ToList();
        qX.RemoveAt(i);
        qY.RemoveAt(i);
        qZ.RemoveAt(i);
        qW.RemoveAt(i);
        QX.keys = qX.ToArray();
        QY.keys = qY.ToArray();
        QZ.keys = qZ.ToArray();
        QW.keys = qW.ToArray();

        Reprocess();
    }

    public (Vector3 pos, Quaternion rot) Sample(float t)
    {
        return (new Vector3(VX.Evaluate(t), VY.Evaluate(t), VZ.Evaluate(t)), new Quaternion(QX.Evaluate(t), QY.Evaluate(t), QZ.Evaluate(t), QW.Evaluate(t)).normalized);
    }

    private void Reprocess()
    {
        var x = VX.keys;
        var y = VY.keys;
        var z = VZ.keys;
        var qX = QX.keys;
        var qY = QY.keys;
        var qZ = QZ.keys;
        var qW = QW.keys;
        for (int i = 0; i < x.Length; i++)
        {
            x[i] = new Keyframe(i / ((float)x.Length - 1), x[i].value);
            y[i] = new Keyframe(i / ((float)x.Length - 1), y[i].value);
            z[i] = new Keyframe(i / ((float)x.Length - 1), z[i].value);
            qX[i] = new Keyframe(i / ((float)x.Length - 1), qX[i].value);
            qY[i] = new Keyframe(i / ((float)x.Length - 1), qY[i].value);
            qZ[i] = new Keyframe(i / ((float)x.Length - 1), qZ[i].value);
            qW[i] = new Keyframe(i / ((float)x.Length - 1), qW[i].value);
        }
        VX.keys = x;
        VY.keys = y;
        VZ.keys = z;
        QX.keys = qX;
        QY.keys = qY;
        QZ.keys = qZ;
        QW.keys = qW;
#if UNITY_EDITOR
        for (int i = 0; i < x.Length; i++)
        {
            if (i != 0)
            {
                UnityEditor.AnimationUtility.SetKeyLeftTangentMode(VX, i, UnityEditor.AnimationUtility.TangentMode.Auto);
                UnityEditor.AnimationUtility.SetKeyLeftTangentMode(VY, i, UnityEditor.AnimationUtility.TangentMode.Auto);
                UnityEditor.AnimationUtility.SetKeyLeftTangentMode(VZ, i, UnityEditor.AnimationUtility.TangentMode.Auto);
                UnityEditor.AnimationUtility.SetKeyLeftTangentMode(QX, i, UnityEditor.AnimationUtility.TangentMode.Auto);
                UnityEditor.AnimationUtility.SetKeyLeftTangentMode(QY, i, UnityEditor.AnimationUtility.TangentMode.Auto);
                UnityEditor.AnimationUtility.SetKeyLeftTangentMode(QZ, i, UnityEditor.AnimationUtility.TangentMode.Auto);
                UnityEditor.AnimationUtility.SetKeyLeftTangentMode(QW, i, UnityEditor.AnimationUtility.TangentMode.Auto);
            }

            if (i != Count - 1)
            {
                UnityEditor.AnimationUtility.SetKeyRightTangentMode(VX, i, UnityEditor.AnimationUtility.TangentMode.Auto);
                UnityEditor.AnimationUtility.SetKeyRightTangentMode(VY, i, UnityEditor.AnimationUtility.TangentMode.Auto);
                UnityEditor.AnimationUtility.SetKeyRightTangentMode(VZ, i, UnityEditor.AnimationUtility.TangentMode.Auto);
                UnityEditor.AnimationUtility.SetKeyRightTangentMode(QX, i, UnityEditor.AnimationUtility.TangentMode.Auto);
                UnityEditor.AnimationUtility.SetKeyRightTangentMode(QY, i, UnityEditor.AnimationUtility.TangentMode.Auto);
                UnityEditor.AnimationUtility.SetKeyRightTangentMode(QZ, i, UnityEditor.AnimationUtility.TangentMode.Auto);
                UnityEditor.AnimationUtility.SetKeyRightTangentMode(QW, i, UnityEditor.AnimationUtility.TangentMode.Auto);
            }
        }
#else
        throw new NotImplementedException();
#endif
    }
}