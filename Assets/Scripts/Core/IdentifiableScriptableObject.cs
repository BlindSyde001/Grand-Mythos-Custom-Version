using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// We expect this asset to be unique and immutable at runtime, it is automatically added to the <see cref="IdentifiableDatabase"/>
/// </summary>
public class IdentifiableScriptableObject : ScriptableObject, ISerializationCallbackReceiver
{
    /// <summary>
    /// Basically, this is because we're saving items through their GUIDs,
    /// since the GUID is the asset id, when a new item is created, even if the name matches, the GUID won't.
    /// You have to manually edit the GUID in the asset file to match with the previous version of the asset.
    /// </summary>
    const string InfoBoxWarning =
        "Deleting then re-creating the same asset will break that asset in saves.\n" +
        "Talk to a programmer if you ever mistakenly do so.";

    public guid Guid => _guid;

    [InfoBox(InfoBoxWarning, InfoMessageType.Warning)]
    [SerializeField, DisplayAsString]
    guid _guid;

    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        if (UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(GetInstanceID(), out string stringGUID, out long fileId/* † */) && UnityEditor.GUID.TryParse(stringGUID, out var result))
        {
            var previousGuid = _guid;
            // †: afaict we don't need fileId, it's for when the asset contains sub-assets ... ? Which doesn't happen for items
            _guid = result;
            if (previousGuid != _guid)
                UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }

    public void OnAfterDeserialize()
    {
        IdentifiableDatabase.EnsureRegistered(this);
    }
}