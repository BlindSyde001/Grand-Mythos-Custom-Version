﻿using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ActionAnimation
{
    public class Sequence : IActionAnimation
    {
        [InfoBox("Plays the following animations in sequence; when the first one finishes, the second starts.")]
        [SerializeReference]
        public IActionAnimation[] Animations = Array.Empty<IActionAnimation>();

        public IEnumerable Play(IAction action, BattleCharacterController controller, TargetCollection targets)
        {
            foreach (var animation in Animations)
            {
                foreach (var yield in animation.Play(action, controller, targets))
                {
                    yield return yield;
                }
            }
        }

        public bool Validate([MaybeNull]IAction action, CharacterTemplate template, ref string message)
        {
            foreach (var actionAnimation in Animations)
            {
                if (actionAnimation.Validate(action, template, ref message) == false)
                    return false;
            }

            return true;
        }
    }
}