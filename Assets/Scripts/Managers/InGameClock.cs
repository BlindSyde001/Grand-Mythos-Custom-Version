using System;
using System.Diagnostics;
using UnityEngine;

public class InGameClock : MonoBehaviour, ISaved<InGameClock, InGameClock.SaveV1>
{
    public static readonly guid Guid = new guid("ae74727b-3be1-48d8-9227-4b3eaf99a272");

    public TimeSpan DurationTotal => _stopwatch.Elapsed + _lastPlaytime;
    TimeSpan _lastPlaytime;
    Stopwatch _stopwatch = Stopwatch.StartNew();

    void Start()
    {
        SavingSystem.TryRestore<InGameClock, SaveV1>(this);
    }

    void OnDestroy()
    {
        SavingSystem.StoreAndUnregister<InGameClock, SaveV1>(this);
    }

    public guid UniqueConstID => Guid;

    [Serializable] public struct SaveV1 : ISaveHandler<InGameClock>
    {
        public TimeSpan TimeSpan => TimeSpan.FromTicks(Ticks);
        public long Ticks;

        public uint Version => 1;
        public void Transfer(InGameClock source, SavingSystem.Transfer transfer)
        {
            if (transfer == SavingSystem.Transfer.PullFromSource)
            {
                Ticks = source.DurationTotal.Ticks;
            }
            else
            {
                source._lastPlaytime = TimeSpan.FromTicks(Ticks);
                source._stopwatch.Restart();
            }
        }
    }
}
