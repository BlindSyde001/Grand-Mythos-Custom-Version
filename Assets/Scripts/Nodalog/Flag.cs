using System;
using UnityEngine;

namespace Nodalog
{
    [CreateAssetMenu(menuName = "Nodalog/Flag")]
    public class Flag : Variable, ISaved<Flag, Flag.Save>
    {
        public bool State;

        guid ISaved.UniqueConstID => Guid;

        protected override void OnEnable()
        {
            SavingSystem.TryRestore<Flag, Save>(this);
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            SavingSystem.StoreAndUnregister<Flag, Save>(this);
            base.OnDisable();
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