using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

public class HeroExtension : CharacterTemplate, ISaved<HeroExtension, HeroExtension.SaveV3>, ISerializationCallbackReceiver
{
    [SerializeField, TitleGroup("EQUIPMENT ATTRIBUTES"), HorizontalGroup("EQUIPMENT ATTRIBUTES/Split"), VerticalGroup("EQUIPMENT ATTRIBUTES/Split/Left"), BoxGroup("EQUIPMENT ATTRIBUTES/Split/Left/Equipment"), LabelWidth(100)]
    protected internal Weapon _Weapon;

    [SerializeField, BoxGroup("EQUIPMENT ATTRIBUTES/Split/Left/Equipment")]
    internal Weapon.WeaponType myWeaponType;

    [SerializeField, BoxGroup("EQUIPMENT ATTRIBUTES/Split/Left/Equipment"), LabelWidth(100)]
    protected internal Armour _Armour;

    [SerializeField, BoxGroup("EQUIPMENT ATTRIBUTES/Split/Left/Equipment")]
    internal Armour.ArmourType myArmourType;

    [SerializeField, BoxGroup("EQUIPMENT ATTRIBUTES/Split/Left/Equipment"), LabelWidth(100)]
    protected internal Accessory _AccessoryOne;

    [SerializeField, BoxGroup("EQUIPMENT ATTRIBUTES/Split/Left/Equipment"), LabelWidth(100)]
    protected internal Accessory _AccessoryTwo;

    [SerializeField, VerticalGroup("EQUIPMENT ATTRIBUTES/Split/Right"), BoxGroup("EQUIPMENT ATTRIBUTES/Split/Right/Total Stats"), LabelWidth(120)]
    private protected int equipHP, equipMP, equipAttack, equipMagAttack, equipDefense,  equipMagDefense, equipSpeed;

    public int EquipHP => equipHP;
    public int EquipMP => equipMP;
    public int EquipAttack => equipAttack;
    public int EquipMagAttack => equipMagAttack;
    public int EquipDefense => equipDefense;
    public int EquipMagDefense => equipMagDefense;
    public int EquipSpeed => equipSpeed;

    [FormerlySerializedAs("charBanner"), HorizontalGroup("ASSETS"), SerializeField, PreviewField(100)]
    internal Sprite Banner;

    [Required, BoxGroup("SKILLS")] public SkillTree SkillTree;

    [BoxGroup("SKILLS")] public SerializableHashSet<guid> UnlockedTreeNodes = new();

    [ReadOnly, BoxGroup("SKILLS")] public SerializableDictionary<guid, IModifier> SkillModifiers = new();
    [BoxGroup("SKILLS")] public IActionCollection Actionset1 = new(), Actionset2 = new();

    public int SkillPointsTotal => Level;

    public override Stats EffectiveStats
    {
        get
        {
            var stats = base.EffectiveStats;
            stats.HP += equipHP;
            stats.MP += equipMP;
            stats.Attack += equipAttack;
            stats.MagAttack += equipMagAttack;
            stats.Defense += equipDefense;
            stats.MagDefense += equipMagDefense;
            stats.Speed += equipSpeed;
            foreach (var modifier in Modifiers)
                modifier.ModifyStats(ref stats);
            return stats;
        }
    }

    // UPDATES
    protected override void Awake()
    {
        SavingSystem.TryRestore<HeroExtension, SaveV3>(this);
        InitializeCharacter();
        RefreshEquipmentStats();
        base.Awake();
    }

    void OnDestroy()
    {
        SavingSystem.StoreAndUnregister<HeroExtension, SaveV3>(this);
    }

    // METHODS
    #region Initialization
    protected void InitializeCharacter()
    {
        LevelUpCheck();
        var nodes = SkillTree.GetNodes();
        foreach (guid guid in UnlockedTreeNodes)
        {
            if (nodes.TryGetValue(guid, out var node))
                node.Unlock.OnUnlock(this, guid);
            else
                MessageModal.Show($"Failed to find node '{guid}'", $"Could not find node '{guid}', this is likely because the skill tree for character '{Name}' has changed, you will have to re-assign some of your points", MessageModal.Type.Warning);
        }

        var matchesOnly = UnlockedTreeNodes.Where(x => nodes.ContainsKey(x)).ToArray();
        UnlockedTreeNodes.Clear();
        foreach (var guid in matchesOnly)
            UnlockedTreeNodes.Add(guid);
    }
    #endregion
    #region Stats & Levelling Up
    internal void RefreshEquipmentStats()
    {
        #region Reset Equip Stats
        equipAttack = 0;
        equipMagAttack = 0;
        equipDefense = 0;
        equipMagDefense = 0;

        equipHP = 0;
        equipMP = 0;
        #endregion
        #region Add Equipped Items to a Temporary List
        var tempEquip = new List<Equipment>();
        tempEquip.Add(_Weapon);
        if (_Armour != null)
            tempEquip.Add(_Armour);
        if (_AccessoryOne != null)
            tempEquip.Add(_AccessoryOne);
        if (_AccessoryTwo != null)
            tempEquip.Add(_AccessoryTwo);
        #endregion
        foreach(Equipment gear in tempEquip)
        {
            equipAttack += gear._EquipAttack;
            equipMagAttack += gear._EquipMagAttack;
            equipDefense += gear._EquipDefense;
            equipMagDefense += gear._EquipMagDefense;

            equipHP += gear._EquipHP;
            equipMP += gear._EquipMP;
        }
    }
    #endregion


    [InfoBox("Each character must have a unique GUID, characters will share GUID when duplicated, in which case you must press the button below on the new character", InfoMessageType.Warning)]
    [SerializeField, DisplayAsString]
    guid _guid;

    public guid Guid => _guid;

    guid ISaved.UniqueConstID => _guid;

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
#if UNITY_EDITOR
        if (_guid == default)
            _guid = System.Guid.NewGuid();
#endif
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize() => PlayableCharacters.EnsureRegistered(this);

    [Button("Generate new GUID", ButtonSizes.Small)]
    void NewGuid()
    {
        _guid = System.Guid.NewGuid();
    }

    [Serializable] public struct SaveV3 : ISaveDataVersioned<SaveV2>, ISaveHandler<HeroExtension>
    {
        public uint Version => 3;

        public int CurrentHP, CurrentMP;
        public int Experience;
        public guid Weapon;
        public guid Armour;
        public guid AccessoryOne;
        public guid AccessoryTwo;
        public SerializableHashSet<guid> AllocatedTreeNodes;
        public List<guid> ActionSet1;
        public List<guid> ActionSet2;

        public void Transfer(HeroExtension source, SavingSystem.Transfer transfer)
        {
            transfer.Value(ref CurrentHP, ref source.CurrentHP);
            transfer.Value(ref CurrentMP, ref source.CurrentMP);
            transfer.Value(ref Experience, ref source.Experience);
            transfer.Identifiable(ref Weapon, ref source._Weapon);
            transfer.Identifiable(ref Armour, ref source._Armour);
            transfer.Identifiable(ref AccessoryOne, ref source._AccessoryOne);
            transfer.Identifiable(ref AccessoryTwo, ref source._AccessoryTwo);
            transfer.Collection<SerializableHashSet<guid>, guid>(ref AllocatedTreeNodes, ref source.UnlockedTreeNodes);
            transfer.Collection(ref ActionSet1, ref source.Actionset1);
            transfer.Collection(ref ActionSet2, ref source.Actionset2);
        }

        public void UpgradeFromPrevious(SaveV2 old)
        {
            CurrentHP = old.CurrentHP;
            CurrentMP = old.CurrentMP;
            Experience = old.Experience;
            Weapon = old.Weapon;
            Armour = old.Armour;
            AccessoryOne = old.AccessoryOne;
            AccessoryTwo = old.AccessoryTwo;
            AllocatedTreeNodes = old.AllocatedTreeNodes;
        }
    }

    [Serializable] public struct SaveV2 : ISaveDataVersioned<SaveV1>
    {
        public uint Version => 2;

        public int CurrentHP, CurrentMP;
        public int Experience;
        public guid Weapon;
        public guid Armour;
        public guid AccessoryOne;
        public guid AccessoryTwo;
        public SerializableHashSet<guid> AllocatedTreeNodes;

        public void Transfer(HeroExtension source, SavingSystem.Transfer transfer)
        {
            transfer.Value(ref CurrentHP, ref source.CurrentHP);
            transfer.Value(ref CurrentMP, ref source.CurrentMP);
            transfer.Value(ref Experience, ref source.Experience);
            transfer.Identifiable(ref Weapon, ref source._Weapon);
            transfer.Identifiable(ref Armour, ref source._Armour);
            transfer.Identifiable(ref AccessoryOne, ref source._AccessoryOne);
            transfer.Identifiable(ref AccessoryTwo, ref source._AccessoryTwo);
            transfer.Collection<SerializableHashSet<guid>, guid>(ref AllocatedTreeNodes, ref source.UnlockedTreeNodes);
        }

        public void UpgradeFromPrevious(SaveV1 old)
        {
            CurrentHP = old.CurrentHP;
            CurrentMP = old.CurrentMP;
            Experience = old.Experience;
            Weapon = old.Weapon;
            Armour = old.Armour;
            AccessoryOne = old.AccessoryOne;
            AccessoryTwo = old.AccessoryTwo;
        }
    }

    [Serializable] public struct SaveV1 : ISaveData
    {
        public uint Version => 1;

        public int CurrentHP, CurrentMP;
        public int Experience;
        public guid Weapon;
        public guid Armour;
        public guid AccessoryOne;
        public guid AccessoryTwo;
        public List<guid> Skills;

        public void Transfer(HeroExtension source, SavingSystem.Transfer transfer)
        {
            transfer.Value(ref CurrentHP, ref source.CurrentHP);
            transfer.Value(ref CurrentMP, ref source.CurrentMP);
            transfer.Value(ref Experience, ref source.Experience);
            transfer.Identifiable(ref Weapon, ref source._Weapon);
            transfer.Identifiable(ref Armour, ref source._Armour);
            transfer.Identifiable(ref AccessoryOne, ref source._AccessoryOne);
            transfer.Identifiable(ref AccessoryTwo, ref source._AccessoryTwo);
            transfer.Identifiables<List<guid>, SerializableHashSet<Skill>, Skill>(ref Skills, ref source.Skills);
        }
    }
}