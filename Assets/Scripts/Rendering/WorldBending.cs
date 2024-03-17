using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[AddComponentMenu(" GrandMythos/WorldBending")]
public class WorldBending : MonoBehaviour
{
    static WorldBending _instance;
    const string Keyword = "WorldBendingOn";
    const string SParamSource = "WorldBendingSource";
    const string SParamCurve = "WorldBendingCurve";
    const string SParamCurveStrength = "WorldBendingCurveStrength";

    public bool PreviewInEditorCamera;
    public Relativity Relative;
    public float Curvature = 2f;
    public float CurvatureStrength = 0.0005f;

    [Header("Wrap Around")]
    public float BoundsPadding = 0.01f;
    public List<GameObject> Mirrors = new();
    [SerializeField, ReadOnly] Bounds _bounds;

    public Bounds Bounds => _bounds;

    void OnEnable()
    {
        if (_instance != null)
            Debug.LogError($"Multiple instances of {nameof(WorldBending)} used");

        _instance = this;
        Camera.onPreCull += OnAnyCameraPreCull;
        Shader.SetKeyword(GlobalKeyword.Create(Keyword), true);
    }

    void OnDisable()
    {
        _instance = null;
        Camera.onPreCull -= OnAnyCameraPreCull;
        Shader.SetKeyword(GlobalKeyword.Create(Keyword), false);
    }

    void OnDrawGizmos()
    {
        if(Bounds is {} val)
            Gizmos.DrawWireCube(val.center, val.size);
    }

    [Button]
    void CreateMirrors()
    {
        ClearMirrors();
        var renderers = new List<Renderer>();
        var surfaces = new List<NavMeshSurface>();
        foreach (var surface in NavMeshSurface.activeSurfaces)
        {
            if (surface.useGeometry != NavMeshCollectGeometry.RenderMeshes)
            {
                Debug.LogError($"WorldBending does not handle {nameof(NavMeshSurface)} set to {surface.useGeometry}", surface);
                continue;
            }
            if (surface.collectObjects != CollectObjects.Children)
            {
                Debug.LogError($"WorldBending does not handle {nameof(NavMeshSurface)} set to {surface.collectObjects}", surface);
                continue;
            }

            surfaces.Add(surface);
            renderers.AddRange(surface.GetComponentsInChildren<Renderer>());
        }

        if (renderers.Count == 0)
            return;

        var bounds = renderers[0].bounds;
        foreach (var renderer in renderers)
        {
            var newBounds = renderer.bounds;
            newBounds.min = Vector3.Min(newBounds.min, bounds.min);
            newBounds.max = Vector3.Min(newBounds.max, bounds.max);
            bounds = newBounds;
        }

        bounds.extents -= new Vector3(BoundsPadding, 0f, BoundsPadding);

        Span<(int, int)> mirrors = stackalloc (int, int)[]
        {
            (-1, -1),
            (-1, +0),
            (-1, +1),
            (+0, -1),
            //(+0, +0), // The center is the reference geometry models, we don't copy over there
            (+0, +1),
            (+1, -1),
            (+1, +0),
            (+1, +1),
        };

        _bounds = bounds;
        var boundsSize = bounds.size;
        foreach (var surface in surfaces)
        {
            var directChildren = new List<Transform>();
            for (int i = 0; i < surface.transform.childCount; i++)
                directChildren.Add(surface.transform.GetChild(i));

            var parent = new GameObject("Mirrors");
            parent.transform.parent = surface.transform;
            Mirrors.Add(parent);
            #if UNITY_EDITOR
            UnityEditor.SceneVisibilityManager.instance.DisablePicking(parent, true);
            #endif
            foreach (var directChild in directChildren)
            {
                foreach ((int x, int z) in mirrors)
                {
                    var copy = Instantiate(directChild.gameObject, parent.transform);
                    copy.transform.position += new Vector3(boundsSize.x * x, 0f, boundsSize.z * z);
                    Mirrors.Add(copy);
                    foreach (var component in copy.GetComponentsInChildren<Component>())
                    {
                        if (component is Renderer or MeshFilter or Transform)
                            continue;
                        Destroy(component);
                    }
                }
            }
        }
#if UNITY_EDITOR
        Unity.AI.Navigation.Editor.NavMeshAssetManager.instance.StartBakingSurfaces(surfaces.Select(x => (UnityEngine.Object)x).ToArray());
#endif
    }

    [Button]
    void ClearMirrors()
    {
        _bounds = default;
        foreach (var mirror in Mirrors)
        {
            if (mirror != null)
            {
                if (Application.isPlaying)
                    Destroy(mirror.gameObject);
                else
                    DestroyImmediate(mirror.gameObject);
            }
        }

        Mirrors.Clear();
    }

    void OnAnyCameraPreCull(Camera cam)
    {
        if (PreviewInEditorCamera == false && cam.name == "SceneCamera")
        {
            Shader.SetKeyword(GlobalKeyword.Create(Keyword), false);
            cam.ResetCullingMatrix();
            return;
        }

        Shader.SetKeyword(GlobalKeyword.Create(Keyword), true);
        Vector3 vec = Relative switch
        {
            Relativity.ToCamera => cam.transform.position,
            Relativity.ToComponent => this.transform.position,
            _ => throw new ArgumentOutOfRangeException(nameof(Relative))
        };
        Shader.SetGlobalVector(SParamSource, new Vector4(vec.x, vec.y, vec.z, 1));
        Shader.SetGlobalFloat(SParamCurve, Curvature);
        Shader.SetGlobalFloat(SParamCurveStrength, CurvatureStrength);
    }

    public static bool ShouldWrapAround(Vector3 worldPosition, out Vector3 newPosition)
    {
        if (_instance && _instance.Bounds != default)
        {
            var val = _instance.Bounds;
            worldPosition.y = val.center.y;
            if (val.Contains(worldPosition) == false)
            {
                newPosition = worldPosition;
                var delta = worldPosition - val.center;
                var absDelta = new Vector3(MathF.Abs(delta.x), MathF.Abs(delta.y), MathF.Abs(delta.z));
                for (int i = 0; i <= 2; i += 2)
                {
                    if (absDelta[i] > val.extents[i])
                    {
                        var howManyWraps = Mathf.CeilToInt(MathF.Abs(delta[i]) / val.size[i]);
                        newPosition[i] -= val.size[i] * Mathf.Sign(delta[i]) * howManyWraps;
                    }
                }

                return true;
            }
        }

        newPosition = default;
        return false;
    }

    public enum Relativity
    {
        ToCamera,
        ToComponent,
    }
}