using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Interactables
{
    [Serializable]
    public class MultiInteraction : IInteraction
    {
        [SerializeReference] public required IInteraction[] Array = System.Array.Empty<IInteraction>();
        public Mode Execution = Mode.Sequentially;

        public enum Mode
        {
            Sequentially,
            Simultaneously
        }

        public IEnumerable<Delay> InteractEnum(IInteractionSource source, OverworldPlayerController player)
        {
            switch (Execution)
            {
                case Mode.Sequentially:
                    foreach (var interaction in Array)
                    {
                        foreach (var yields in interaction.InteractEnum(source, player))
                            yield return yields;
                    }
                    break;
                case Mode.Simultaneously:
                    var enums = new IEnumerator<Delay>?[Array.Length];
                    for (int i = 0; i < enums.Length; i++)
                        enums[i] = Array[i].InteractEnum(source, player).GetEnumerator();

                    int idles = 0;
                    do
                    {
                        for (int i = 0; i < enums.Length; i++)
                        {
                            var enumVal = enums[i];
                            if (enumVal != null && enumVal.MoveNext())
                            {
                                switch (enumVal.Current)
                                {
                                    case Delay.WaitTillNextFrame:
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException(enumVal.Current.ToString());
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

        public bool IsValid([MaybeNullWhen(true)]out string error)
        {
            for (int i = 0; i < Array.Length; i++)
            {
                IInteraction? interaction = Array[i];
                if (interaction == null!)
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