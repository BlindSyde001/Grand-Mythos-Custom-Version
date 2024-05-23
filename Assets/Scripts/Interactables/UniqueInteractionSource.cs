using System;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class UniqueInteractionSource : MonoBehaviour, IInteractionSource, ISaved<UniqueInteractionSource, UniqueInteractionSource.Save>
{
    const string InfoBoxWarning =
        "This value identifies this component for saving and restoring its state when its Type is set to 'Once Ever',\n" +
        "if you delete it and recreate one, any saved state won't transfer to the new one.\n" +
        "Talk to a programmer if you ever mistakenly do so.";

    [InfoBox(InfoBoxWarning, InfoMessageType.Warning)]
    [SerializeField, DisplayAsString]
    guid _guid = Guid.NewGuid();

    [Required, SerializeReference, SerializeField]
    protected IInteraction Interaction;

    public TriggerType Type = TriggerType.OnceEveryLoad;

    bool _consumed;

    public guid UniqueConstID => _guid;
    /// <summary> Was this event triggered </summary>
    public bool Consumed => _consumed;

    void Awake()
    {
        if (Type == TriggerType.OnceEver)
            SavingSystem.TryRestore<UniqueInteractionSource, Save>(this);

        if (_consumed)
            enabled = false;
    }

    void OnDestroy()
    {
        if (Type == TriggerType.OnceEver)
            SavingSystem.StoreAndUnregister<UniqueInteractionSource, Save>(this);
    }

    public bool TryConsumeInteraction(out IInteraction interaction)
    {
        if (_consumed && Type is TriggerType.OnceEver or TriggerType.OnceEveryLoad)
        {
            interaction = null;
            return false;
        }

        if (Interaction == null)
            Debug.LogError($"No interaction on this interactable ({this})", this);

        _consumed = true;
        interaction = Interaction;
        return true;
    }

    public enum TriggerType
    {
        Always,
        OnceEveryLoad,
        OnceEver
    }

    [Serializable] public struct Save : ISaveHandler<UniqueInteractionSource>
    {
        public bool Consumed;

        public uint Version => 1;

        public void Transfer(UniqueInteractionSource source, SavingSystem.Transfer transfer)
        {
            transfer.Value(ref Consumed, ref source._consumed);
        }
    }
}