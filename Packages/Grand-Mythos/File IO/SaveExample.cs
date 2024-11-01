using System;

[Serializable] public class SaveExample : ISaved<SaveExample, SaveExample.HandlerV3>
{
    public int Health, Mana;
    public bool Alive;
    public float HeadSize;
    public SerializableHashSet<int> KnownNumbers;

    public guid UniqueConstID { get; } = new("42def988-ade2-4d6e-831a-3880a48a5e82");

    public void Unload()
    {
        SavingSystem.StoreAndUnregister<SaveExample, HandlerV3>(this);
    }

    public void Load()
    {
        SavingSystem.TryRestore<SaveExample, HandlerV3>(this);
    }

    [Serializable] public struct HandlerV3 : ISaveDataVersioned<HandlerV2>, ISaveHandler<SaveExample>
    {
        public HandlerV2 BaseStats;
        public float HeadSize;
        public SerializableHashSet<int> KnownNumbers;

        public uint Version => 3;

        public void Transfer(SaveExample source, SavingSystem.Transfer transfer)
        {
            transfer.Value(ref BaseStats.Health, ref source.Health);
            transfer.Value(ref BaseStats.Mana, ref source.Mana);
            transfer.Value(ref BaseStats.Alive, ref source.Alive);
            transfer.Value(ref HeadSize, ref source.HeadSize);
            transfer.Collection<SerializableHashSet<int>, int>(ref KnownNumbers, ref source.KnownNumbers);
        }

        public void UpgradeFromPrevious(HandlerV2 old)
        {
            BaseStats = old;
            HeadSize = 1f;
        }
    }

    [Serializable] public struct HandlerV2 : ISaveDataVersioned<HandlerV1>
    {
        public int Health, Mana;
        public bool Alive;

        public uint Version => 2;

        public void UpgradeFromPrevious(HandlerV1 old)
        {
            Health = (int)old.Health;
            Mana = (int)old.Mana;
            Alive = true;
        }
    }

    [Serializable] public struct HandlerV1 : ISaveData
    {
        public uint Version => 1;

        public uint Health, Mana;
    }
}