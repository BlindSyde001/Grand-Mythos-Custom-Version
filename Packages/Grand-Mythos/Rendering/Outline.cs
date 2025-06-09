using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class Outline : MonoBehaviour
{
    private static bool _recursionSafeguard;
    [Required, OnValueChanged(nameof(Apply))] public Material OutlineMaterial;
    [ReadOnly] public Renderer[] OutlineObjects = Array.Empty<Renderer>();

    private void OnEnable()
    {
        if (_recursionSafeguard)
            return;
        Apply();
    }

    private void OnDisable()
    {
        foreach (var o in OutlineObjects)
            if (o != null)
                DestroyImmediate(o.gameObject);
    }

    [Button]
    private void Apply()
    {
        foreach (var o in OutlineObjects)
            if (o != null)
                DestroyImmediate(o.gameObject);

        var renderers = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            Renderer r;
            try
            {
                _recursionSafeguard = true;
                r = Instantiate(renderer, renderer.transform);
            }
            finally
            {
                _recursionSafeguard = false;
            }
            
            r.material = OutlineMaterial;
            r.shadowCastingMode = ShadowCastingMode.Off;
            for (int j = r.gameObject.GetComponentCount() - 1; j >= 0; j--)
            {
                var c = r.gameObject.GetComponentAtIndex(j);
                if (c is Transform or Renderer or MeshFilter)
                    continue;
                DestroyImmediate(c);
            }

            if (r is SkinnedMeshRenderer smr)
            {
                var m = Instantiate(smr.sharedMesh);
                AverageNormals(m);
                smr.sharedMesh = m;
            }

            r.gameObject.hideFlags |= HideFlags.HideAndDontSave;
            renderers[i] = r;
        }

        OutlineObjects = renderers;

        static void AverageNormals(Mesh m)
        {
            var n = m.normals;
            var p = m.vertices;

            var sharedPositions = new Dictionary<Vector3, List<int>>();
            for (int j = 0; j < p.Length; j++)
            {
                if (sharedPositions.TryGetValue(p[j], out var indices) == false)
                    sharedPositions[p[j]] = indices = new();
                indices.Add(j);
            }

            foreach (var (_, indices) in sharedPositions)
            {
                var normalAverage = new Vector3();
                foreach (var index in indices)
                    normalAverage += n[index];
                normalAverage.Normalize();
                foreach (var index in indices)
                    n[index] = normalAverage;
            }

            m.normals = n;
        }
    }
}