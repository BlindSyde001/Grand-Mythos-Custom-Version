using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Nodalog
{
    [CreateAssetMenu(menuName = "Nodalog/Tally")]
    public class Tally : Variable, ISaved<Tally, Tally.Save>
    {
        [OnValueChanged(nameof(OnInspectorChange))]
        public int Amount;

        [SerializeField, ReadOnly]
        int _defaultValue;

        guid ISaved.UniqueConstID => Guid;

        void OnInspectorChange()
        {
            if (Application.isPlaying == false)
                _defaultValue = Amount;
        }

        protected override void OnPlay()
        {
            SavingSystem.TryRestore<Tally, Save>(this);
        }

        protected override void OnExit()
        {
            if (Amount != _defaultValue)
                SavingSystem.StoreAndUnregister<Tally, Save>(this);
            else
                SavingSystem.Unregister<Tally, Save>(this);
            Amount = _defaultValue;
        }

        [Serializable] public struct Save : ISaveHandler<Tally>
        {
            public int Amount;

            public uint Version => 1;

            public void Transfer(Tally source, SavingSystem.Transfer transfer)
            {
                transfer.Value(ref Amount, ref source.Amount);
            }
        }
    }
}