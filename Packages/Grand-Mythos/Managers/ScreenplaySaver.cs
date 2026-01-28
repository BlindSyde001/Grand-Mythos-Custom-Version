using System;
using Screenplay;
using Screenplay.Component;
using UnityEngine;

public class ScreenplaySaver : MonoBehaviour, ISaved<ScreenplaySaver, ScreenplaySaver.HandlerV1>
{
    public required ScreenplayDispatcher Dispatcher;
    
    public guid UniqueConstID => new("bcfece79-69b0-42da-acb7-751461d39ec5");

    public void Awake()
    {
        SavingSystem.TryRestore<ScreenplaySaver, HandlerV1>(this);
    }

    public void OnDestroy()
    {
        SavingSystem.StoreAndUnregister<ScreenplaySaver, HandlerV1>(this);
    }

    [Serializable] public struct HandlerV1 : ISaveData, ISaveHandler<ScreenplaySaver>
    {
        public uint Version => 1;

        public ScreenplayGraph.State State;

        public void Transfer(ScreenplaySaver source, SavingSystem.Transfer transfer)
        {
            if (transfer == SavingSystem.Transfer.PullFromSource)
            {
                var data = source.Dispatcher.SaveState();
                State = data;
            }
            else
            {
                if (source.Dispatcher == null!)
                {
                    Debug.LogError("Missing screenplay dispatcher", source);
                    return;
                }

                source.Dispatcher.SetStateToLoad(State);
            }
        }
    }
}