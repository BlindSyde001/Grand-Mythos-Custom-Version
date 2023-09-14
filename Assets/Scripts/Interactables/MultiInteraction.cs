using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Interactables
{
    [Serializable]
    public class MultiInteraction : IInteraction
    {
        [Required, SerializeReference] public IInteraction[] Array = System.Array.Empty<IInteraction>();
        public Mode Execution = Mode.Sequentially;

        public enum Mode
        {
            Sequentially,
            Simultaneously
        }

        public IEnumerable<Delay> Interact(IInteractionSource source, OverworldPlayerControlsNode player)
        {
            switch (Execution)
            {
                case Mode.Sequentially:
                    foreach (var interaction in Array)
                    {
                        foreach (var yields in interaction.Interact(source, player))
                            yield return yields;
                    }
                    break;
                case Mode.Simultaneously:
                    var enums = new IEnumerator<Delay>[Array.Length];
                    for (int i = 0; i < enums.Length; i++)
                        enums[i] = Array[i].Interact(source, player).GetEnumerator();

                    int idles = 0;
                    do
                    {
                        for (int i = 0; i < enums.Length; i++)
                        {
                            if (enums[i] != null && enums[i].MoveNext())
                            {
                                switch (enums[i].Current)
                                {
                                    case Delay.WaitTillNextFrame:
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException(enums[i].Current.ToString());
                                }

                                continue;
                            }

                            idles++;
                            enums[i] = null;
                        }

                        yield return Delay.WaitTillNextFrame;

                    } while (idles != enums.Length);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool IsValid(out string error)
        {
            for (int i = 0; i < Array.Length; i++)
            {
                IInteraction interaction = Array[i];
                if (interaction == null)
                {
                    error = $"Interaction #{i} is null";
                    return false;
                }

                if (interaction.IsValid(out error) == false)
                    return false;
            }

            error = null;
            return true;
        }
    }
}