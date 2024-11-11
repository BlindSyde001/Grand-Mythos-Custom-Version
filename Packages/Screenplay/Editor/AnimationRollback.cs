using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Screenplay
{
    public class AnimationRollback : IAnimationRollback
    {
        readonly (Object target, (EditorCurveBinding prop, object? val)[] arr)[] _data;

        public AnimationRollback(GameObject go, AnimationClip clip)
        {
            _data = GetObjectState(go, clip).ToArray();
        }

        public void Rollback()
        {
            SetValuesBack(_data);
        }

        IEnumerable<(Object target, (EditorCurveBinding prop, object? val)[] arr)> GetObjectState(GameObject root, AnimationClip clip)
        {
            foreach (var group in RetrieveBindings(clip).GroupBy(x => x.path))
            {
                var firstBinding = group.First();
                var target = AnimationUtility.GetAnimatedObject(root, firstBinding);
                if (target == null)
                {
                    Debug.LogWarning($"Could not find {firstBinding.path}");
                    continue;
                }

                var serializedObject = new SerializedObject(target);
                var arr = group.Select(x => (prop: x, val: default(object))).ToArray();
                for (int i = 0; i < arr.Length; i++)
                {
                    var serializedProp = serializedObject.FindProperty(arr[i].prop.propertyName);
                    arr[i].val = serializedProp.boxedValue;
                }

                yield return (target, arr);
            }

            static IEnumerable<EditorCurveBinding> RetrieveBindings(AnimationClip clip)
            {
                foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                    yield return binding;

                foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                    yield return binding;
            }
        }

        void SetValuesBack(IEnumerable<(Object target, (EditorCurveBinding prop, object? val)[] arr)> values)
        {
            foreach ((Object target, (EditorCurveBinding prop, object? val)[] arr) in values)
            {
                var serializedObject = new SerializedObject(target);
                foreach ((EditorCurveBinding prop, object? val) in arr)
                {
                    var serializedProp = serializedObject.FindProperty(prop.propertyName);
                    serializedProp.boxedValue = val;
                }

                serializedObject.ApplyModifiedProperties();
            }
        }

        public void Dispose()
        {
            Rollback();
        }
    }
}
