using System;
using System.Collections.Generic;
using Interactables.Conditions;
using Sirenix.OdinInspector;
using UnityEngine;
using ProcessorSystem;

namespace Interactables
{
    [AddComponentMenu(" GrandMythos/ConditionalObject")]
    public class ConditionalObject : IComponent<ConditionalObjectProcessor, ConditionalObject>
    {
        [Required, SerializeReference, ValidateInput(nameof(ValidateCondition)), InlineProperty, HideLabel, BoxGroup]
        public ICondition Condition = new FlagIs();

        [HideInInspector]
        public bool LastEvaluation;

        bool ValidateCondition(ICondition condition, ref string error)
        {
            return condition?.IsValid(out error) ?? true;
        }

        [Button]
        void TestCondition()
        {
            #if UNITY_EDITOR
            bool value = Condition.Evaluate();
            var debugTxt = $"({DateTime.Now:HH:mm:ss}) {value}; the object will be {(value ? "enabled" : "disabled")} with those conditions in the current context";
            UnityEditor.EditorUtility.DisplayDialog(nameof(TestCondition), debugTxt, "Ok");
            #endif
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Condition.Evaluate() == false)
                gameObject.SetActive(false);
        }
    }

    public class ConditionalObjectProcessor : IComponent<ConditionalObjectProcessor, ConditionalObject>.IProcessor
    {
        public List<ConditionalObject> Components = new();

        public bool AddOnAwakeRemoveOnDestroy => true;

        public void Update()
        {
            #warning ultimately it would be better if it subscribed to changes in the condition variables
            foreach (var element in Components)
            {
                try
                {
                    bool state = element.LastEvaluation;
                    bool newState = element.Condition.Evaluate();
                    element.LastEvaluation = newState;
                    if (newState != state) // If it should be disabled but it is enabled and vice versa
                        element.gameObject.SetActive(newState);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public void Add(ConditionalObject item)
        {
            Components.Add(item);
            try
            {
                item.LastEvaluation = item.Condition.Evaluate();
                item.gameObject.SetActive(item.LastEvaluation);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void Remove(ConditionalObject item)
        {
            Components.Remove(item);
        }
    }
}