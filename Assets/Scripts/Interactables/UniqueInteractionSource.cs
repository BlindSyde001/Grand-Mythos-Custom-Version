﻿using System;
using System.Collections;
using System.Linq;
using Interactables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class UniqueInteractionSource : MonoBehaviour, IInteractionSource, ISaved<UniqueInteractionSource, UniqueInteractionSource.Save>
{
    const string InfoBoxGuidWarning =
        "This value identifies this component for saving and restoring its state when its Type is set to 'Once Ever',\n" +
        "if you delete it and recreate one, any saved state won't transfer to the new one.\n" +
        "Talk to a programmer if you ever mistakenly do so.";

    const string InfoBoxWarningPersistent = "The reaction that runs after 'OnTrigger' and whenever we load a game in which this object has been triggered.\n" +
                                   "For example, this interaction would open the locked door to keep it open between sessions, while OnTrigger would remove the key from the player's inventory";

    [InfoBox(InfoBoxGuidWarning, InfoMessageType.Warning)]
    [SerializeField, DisplayAsString, ReadOnly]
    guid _guid = Guid.NewGuid();

    [FormerlySerializedAs("Interaction"), Required, SerializeReference, SerializeField, ValidateInput(nameof(ValidateOnTrigger))]
    protected IInteraction OnTrigger;

    [SerializeReference, SerializeField, Tooltip(InfoBoxWarningPersistent), ValidateInput(nameof(ValidatePersistentEffect))]
    protected IInteraction PersistentEffect;

    public TriggerType Type = TriggerType.OnceEveryLoad;

    bool _consumed;

    public guid UniqueConstID => _guid;

    /// <summary> Was this event triggered </summary>
    public bool Consumed => _consumed && Type is TriggerType.OnceEver or TriggerType.OnceEveryLoad;

    void Awake()
    {
        if (Type == TriggerType.OnceEver)
            SavingSystem.TryRestore<UniqueInteractionSource, Save>(this);

        if (_consumed && PersistentEffect is not null)
        {
            if (OverworldPlayerController.Instances.Count == 0)
                StartCoroutine(WaitForPlayerAndTriggerPersistentEffect());
            else
                OverworldPlayerController.Instances.First().PlayInteraction(this, PersistentEffect);

            IEnumerator WaitForPlayerAndTriggerPersistentEffect()
            {
                while (OverworldPlayerController.Instances.Count == 0)
                    yield return null;

                OverworldPlayerController.Instances.First().PlayInteraction(this, PersistentEffect);
            }
        }
    }

    void OnDestroy()
    {
        if (Type == TriggerType.OnceEver)
            SavingSystem.StoreAndUnregister<UniqueInteractionSource, Save>(this);
    }

    static bool ValidateOnTrigger(IInteraction interaction, ref string message)
    {
        return interaction != null && interaction.IsValid(out message);
    }

    static bool ValidatePersistentEffect(IInteraction interaction, ref string message)
    {
        return interaction == null || interaction.IsValid(out message);
    }

    public bool TryConsumeAndPlayInteraction(OverworldPlayerController controller)
    {
        if (_consumed && Type is TriggerType.OnceEver or TriggerType.OnceEveryLoad)
        {
            return false;
        }

        if (OnTrigger == null)
            Debug.LogError($"No interaction on this interactable ({this})", this);

        _consumed |= Type is TriggerType.OnceEver or TriggerType.OnceEveryLoad;
        IInteraction interaction;
        if (PersistentEffect is null)
            interaction = OnTrigger;
        else
            interaction = new MultiInteraction { Array = new[] { OnTrigger, PersistentEffect }, Execution = MultiInteraction.Mode.Sequentially };

        controller.PlayInteraction(this, interaction);
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

    static UniqueInteractionSource()
    {
        #if UNITY_EDITOR
        UnityEditor.ObjectChangeEvents.changesPublished += ChangesPublished;
        static void ChangesPublished(ref UnityEditor.ObjectChangeEventStream stream)
        {
            for (int i = 0; i < stream.length; ++i)
            {
                var type = stream.GetEventType(i);
                switch (type)
                {
                    case UnityEditor.ObjectChangeKind.CreateGameObjectHierarchy:
                        stream.GetCreateGameObjectHierarchyEvent(i, out var createGameObjectHierarchyEvent);
                        var newGameObject = (GameObject)UnityEditor.EditorUtility.InstanceIDToObject(createGameObjectHierarchyEvent.instanceId);
                        foreach (var source in newGameObject.GetComponentsInChildren<UniqueInteractionSource>())
                        {
                            source._guid = Guid.NewGuid();
                            Debug.LogWarning($"Assigning new GUID for duplicated object {newGameObject} to ensure it doesn't clash with its original.");
                        }

                        break;
                }
            }
        }
        #endif
    }
}