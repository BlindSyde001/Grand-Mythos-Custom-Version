using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;


public class SceneGUIProxy
{
#if UNITY_EDITOR
    private UnityEditor.IMGUI.Controls.BoxBoundsHandle m_BoundsHandle = new();
#endif

    public static SceneGUIProxy Instance = new();

    public Matrix4x4 matrix
    {
        get
        {
            #if UNITY_EDITOR
            return UnityEditor.Handles.matrix;
            #else
            return Matrix4x4.identity;
            #endif
        }
        set
        {
            #if UNITY_EDITOR
            UnityEditor.Handles.matrix = value;
            #endif
        }
    }

    public Vector3 PositionHandle(Vector3 position, Quaternion rotation)
    {
        #if UNITY_EDITOR
        position = UnityEditor.Handles.PositionHandle(position, rotation);
        #endif
        return position;
    }

    public Quaternion RotationHandle(Quaternion rotation, Vector3 position)
    {
        #if UNITY_EDITOR
        rotation = UnityEditor.Handles.RotationHandle(rotation, position);
        #endif
        return rotation;
    }

    public Vector3 ScaleHandle(Vector3 scale, Vector3 position, Quaternion rotation)
    {
        #if UNITY_EDITOR
        scale = UnityEditor.Handles.ScaleHandle(scale, position, rotation);
        #endif
        return scale;
    }

    public Bounds Bounds(Bounds bounds)
    {
        #if UNITY_EDITOR
        m_BoundsHandle.center = bounds.center;
        m_BoundsHandle.size = bounds.size;
        m_BoundsHandle.DrawHandle();
        bounds.center = m_BoundsHandle.center;
        bounds.size = m_BoundsHandle.size;
        #endif
        return bounds;
    }

    public void Color(Color color)
    {
        #if UNITY_EDITOR
        UnityEditor.Handles.color = color;
        #endif
    }

    public void Line(Vector3 start, Vector3 end)
    {
        #if UNITY_EDITOR
        UnityEditor.Handles.DrawLine(start, end);
        #endif
    }

    public void DottedLine(Vector3 start, Vector3 end, float screenspaceSize)
    {
        #if UNITY_EDITOR
        UnityEditor.Handles.DrawDottedLine(start, end, screenspaceSize);
        #endif
    }

    public void WireCube(Vector3 center, Vector3 size)
    {
        #if UNITY_EDITOR
        UnityEditor.Handles.DrawWireCube(center, size);
        #endif
    }

    public void WireCube(Bounds bounds)
    {
        #if UNITY_EDITOR
        UnityEditor.Handles.DrawWireCube(bounds.center, bounds.size);
        #endif
    }

    public void WireSphere(Vector3 center, float radius)
    {
        #if UNITY_EDITOR
        if (Event.current.type != EventType.Repaint)
            return;

        if (s_SphereLines == null)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s_SphereLines = ConvertToLineMesh(obj.GetComponent<MeshFilter>().sharedMesh);
            Object.DestroyImmediate(obj);
        }

        s_ApplyWireMaterial.Invoke(UnityEditor.Handles.zTest);
        /*Shader.SetGlobalColor("_HandleColor", UnityEditor.Handles.color * new Color(1f, 1f, 1f, 0.5f));
        Shader.SetGlobalFloat("_HandleSize", 1f);
        UnityEditor.HandleUtility.handleMaterial.SetFloat("_HandleZTest", (float)UnityEditor.Handles.zTest);
        UnityEditor.HandleUtility.handleMaterial.SetPass(0);*/
        Graphics.DrawMeshNow(s_SphereLines, UnityEditor.Handles.matrix * Matrix4x4.TRS(center, Quaternion.identity, Vector3.one * (radius * 2f)));

        #endif
    }


    public void Label(Vector3 position, string text, Color? color = null)
    {
        #if UNITY_EDITOR
        if (color != null)
            GUI.color = color.Value;
        UnityEditor.Handles.Label(position, text);
        #endif
    }

    public void RecordObject(Object obj, string name)
    {
        #if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(obj, name);
        #endif
    }

    private static Mesh? s_SphereLines;
    private static Action<CompareFunction> s_ApplyWireMaterial, s_ApplyDottedWireMaterial;

    static Mesh ConvertToLineMesh(Mesh mesh)
    {
        var tris = mesh.triangles;
        List<int> lineIndices = new List<int>(tris.Length * 2);
        for (int i = 0; i < tris.Length; i += 3)
        {
            lineIndices.Add(tris[i]);
            lineIndices.Add(tris[i + 1]);

            lineIndices.Add(tris[i + 1]);
            lineIndices.Add(tris[i + 2]);

            lineIndices.Add(tris[i + 2]);
            lineIndices.Add(tris[i]);
        }
        var lineMesh = new Mesh
        {
            vertices = mesh.vertices,
            uv = mesh.uv,
            normals = mesh.normals,
            tangents = mesh.tangents
        };
        lineMesh.SetIndices(lineIndices, MeshTopology.Lines, 0, true);
        return lineMesh;
    }

    static SceneGUIProxy()
    {
        #if UNITY_EDITOR
        s_ApplyDottedWireMaterial ??= (Action<CompareFunction>)typeof(UnityEditor.HandleUtility)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .First(x => x.Name == "ApplyDottedWireMaterial" && x.GetParameters().Length == 1)
            .CreateDelegate(typeof(Action<CompareFunction>));
        s_ApplyWireMaterial ??= (Action<CompareFunction>)typeof(UnityEditor.HandleUtility)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .First(x => x.Name == "ApplyWireMaterial" && x.GetParameters().Length == 1)
            .CreateDelegate(typeof(Action<CompareFunction>));
        #endif
    }
}
