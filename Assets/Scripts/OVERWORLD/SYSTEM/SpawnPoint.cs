using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu(" GrandMythos/SpawnPoint")]
public class SpawnPoint : MonoBehaviour
{
    public static SerializableDictionary<SpawnPointReference, SpawnPoint> SpawnPoints = new();
    public static SpawnPoint LastSpawnUsed;
    public static SpawnPointReference ScheduledSpawnOnPoint;

    [InlineEditor]
    public SpawnPointReference Reference;

    void Awake()
    {
        if (SpawnPoints.TryGetValue(Reference, out var match) && match != null && match != this)
            Debug.LogError($"Two {nameof(SpawnPoint)} share the same {nameof(SpawnPointReference)}, {this} and {match}", match);
        SpawnPoints[Reference] = this;
    }

    void OnDestroy()
    {
        SpawnPoints.Remove(Reference);
    }

    void OnEnable()
    {
        if (ScheduledSpawnOnPoint == Reference)
        {
            ScheduledSpawnOnPoint = null;
            LastSpawnUsed = this;
            Instantiate(Reference.PlayerCharacter.Prefab, transform.position, transform.rotation);
        }
        else if (OverworldPlayerControlsNode.Instances.Count == 0)
            StartCoroutine(DelayedSpawn());
    }

    IEnumerator DelayedSpawn()
    {
        yield return null; // Wait at least two frames to make sure that everything that meant to spawn the player had the time to do so
        yield return null;
        if (OverworldPlayerControlsNode.Instances.Count != 0)
            yield break;

        LastSpawnUsed = this;
        Instantiate(Reference.PlayerCharacter.Prefab, transform.position, transform.rotation);
        Debug.Log("Fallback spawn for player");
    }

    [Button("Create Reference")]
    void CreateReferenceAsset()
    {
#if UNITY_EDITOR
        var scenePath = gameObject.scene.path;
        var sceneName = gameObject.scene.name;
        var sceneAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(scenePath);
        if (sceneAsset == null)
        {
            UnityEditor.EditorUtility.DisplayDialog(nameof(SpawnPoint), $"Could not find scene {scenePath}, make sure your scene is saved", "Ok");
            return;
        }

        SpawnPointReference reference = ScriptableObject.CreateInstance<SpawnPointReference>();
        reference.SpawnName = gameObject.name;
        reference.Scene = new SceneReference(sceneAsset);
        string[] guids1 = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(SpawnablePlayerCharacter)}", null);
        if (guids1.Length == 0)
        {
            UnityEditor.EditorUtility.DisplayDialog(nameof(SpawnPoint), $"Could not find any {nameof(SpawnablePlayerCharacter)} in the asset database, make sure there is at least one created", "Ok");
            return;
        }

        reference.PlayerCharacter = UnityEditor.AssetDatabase.LoadAssetAtPath<SpawnablePlayerCharacter>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids1[0]));
        var saveLocation = UnityEditor.EditorUtility.SaveFilePanelInProject($"Save {nameof(SpawnPointReference)}", $"{sceneName}_{gameObject.name}", "asset", $"Save {nameof(SpawnPointReference)}");
        if (string.IsNullOrWhiteSpace(saveLocation))
            return;

        bool found = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            if (SceneManager.GetSceneByBuildIndex(i) == gameObject.scene)
                found = true;
        }

        if (found == false)
            UnityEditor.EditorUtility.DisplayDialog(nameof(SpawnPoint), $"Scene '{scenePath}' is not part of the build, you should add it to the build", "Ok");

        UnityEditor.AssetDatabase.CreateAsset(reference, saveLocation);
        Reference = reference;
#endif
    }

    void OnDrawGizmos()
    {
        var center = transform.position + Vector3.up;
        Gizmos.matrix = this.transform.localToWorldMatrix;
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawCube(Vector3.up, new Vector3(0.8f, 2f, 0.8f));

        if (Reference == null)
            GizmosHelper.Label(center, $"No {nameof(Reference)} created for this {nameof(SpawnPoint)}", Color.red);
        else if (Reference.Scene.Path != gameObject.scene.path)
            GizmosHelper.Label(center, $"{nameof(Reference)} scene doesn't match this {nameof(SpawnPoint)}'s scene", Color.red);
        else if (SpawnPoints.TryGetValue(Reference, out var match) && match != null && match != this)
            GizmosHelper.Label(center, $"This {nameof(SpawnPoint)} shares the same {nameof(SpawnPointReference)} with {match}");
        else
            SpawnPoints[Reference] = this;
    }

    static SpawnPoint()
    {
        DomainReloadHelper.BeforeReload += helper =>
        {
            helper._spawnPoints = SpawnPoints;
            helper._lastSpawnUsed = LastSpawnUsed;
        };
        DomainReloadHelper.AfterReload += helper =>
        {
            SpawnPoints = helper._spawnPoints;
            LastSpawnUsed = helper._lastSpawnUsed;
        };
    }
}

public partial class DomainReloadHelper
{
    public SerializableDictionary<SpawnPointReference, SpawnPoint> _spawnPoints;
    public SpawnPoint _lastSpawnUsed;
}