using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Nodalog
{
    [CreateAssetMenu(menuName = "Nodalog/Flag")]
    public class Flag : Variable, ISaved<Flag, Flag.Save>
    {
        [OnValueChanged(nameof(OnInspectorChange))]
        public bool State;

        [SerializeField, ReadOnly]
        bool _defaultValue;

        guid ISaved.UniqueConstID => Guid;

        void OnInspectorChange()
        {
            if (Application.isPlaying == false)
                _defaultValue = State;
        }

        protected override void OnPlay()
        {
            SavingSystem.TryRestore<Flag, Save>(this);
        }

        protected override void OnExit()
        {
            if (State != _defaultValue)
                SavingSystem.StoreAndUnregister<Flag, Save>(this);
            else
                SavingSystem.Unregister<Flag, Save>(this);
            State = _defaultValue;
        }

        [Serializable] public struct Save : ISaveHandler<Flag>
        {
            public bool State;

            public uint Version => 1;

            public void Transfer(Flag source, SavingSystem.Transfer transfer)
            {
                transfer.Value(ref State, ref source.State);
            }
        }
    }
}