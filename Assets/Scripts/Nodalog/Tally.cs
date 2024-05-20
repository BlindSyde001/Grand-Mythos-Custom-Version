using System;
using UnityEngine;

namespace Nodalog
{
    [CreateAssetMenu(menuName = "Nodalog/Tally")]
    public class Tally : Variable, ISaved<Tally, Tally.Save>
    {
        public int Amount;

        guid ISaved.UniqueConstID => Guid;

        protected override void OnEnable()
        {
            SavingSystem.TryRestore<Tally, Save>(this);
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            SavingSystem.StoreAndUnregister<Tally, Save>(this);
            base.OnDisable();
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