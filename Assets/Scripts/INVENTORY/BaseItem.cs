using System;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

public class BaseItem : ScriptableObject, ISerializationCallbackReceiver
{
    /// <summary>
    /// Basically, this is because we're saving items through their GUIDs,
    /// since the GUID is the asset id, when a new item is created, even if the name matches, the GUID won't.
    /// You have to manually edit the GUID to match with the previous version of the asset.
    /// </summary>
    const string InfoBoxWarning =
        "Deleting then re-creating the same item will break saves.\n" +
        "Talk to a programmer if you ever mistakenly do so.";


    [InfoBox(InfoBoxWarning, InfoMessageType.Warning)]
    [FormerlySerializedAs("ItemDescription"),FormerlySerializedAs("_ItemDescription"), TextArea]
    public string Description;

    [HideInInspector, SerializeField]
    public guid guid;

    public unsafe void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        if (UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(GetInstanceID(), out string stringGUID, out long fileId/* † */) && UnityEditor.GUID.TryParse(stringGUID, out var result))
        {
            // †: afaict we don't need fileId, it's for when the asset contains sub-assets ... ? Which doesn't happen for items
            if (sizeof(guid) != sizeof(UnityEditor.GUID))
                throw new Exception($"Our GUID's size ({sizeof(guid)}) does not match unity's ({sizeof(UnityEditor.GUID)})");
            guid = *(guid*)&result;
        }
#endif
    }

    public void OnAfterDeserialize(){ }

    static unsafe BaseItem()
    {
#if UNITY_EDITOR
        if (sizeof(guid) != sizeof(UnityEditor.GUID))
            Debug.LogError("Our GUID does not match unity's");
#endif
    }
}

[Serializable]
public class ItemCapsule
{
    [Required]
    public BaseItem thisItem;
    public int ItemAmount = 1;
}