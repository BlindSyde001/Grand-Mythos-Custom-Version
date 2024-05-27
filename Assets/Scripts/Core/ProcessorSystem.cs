using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProcessorSystem
{
    public class ProcessorsManager : MonoBehaviour
    {
        static ProcessorsManager _instance;

        [SerializeReference] public List<IProcessorBase> Instances = new();

        public static T GetInstance<T>() where T : IProcessorBase, new()
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ProcessorsManager>(true);
                if (_instance == null)
                {
                    _instance = new GameObject(nameof(ProcessorsManager)).AddComponent<ProcessorsManager>();
                    DontDestroyOnLoad(_instance.gameObject);
                }
            }

            foreach (var instance in _instance.Instances)
            {
                if (instance is T instanceOfT)
                    return instanceOfT;
            }

            var output = new T();
            _instance.Instances.Add(output);
            return output;
        }

        void Update()
        {
            foreach (var instance in _instance.Instances)
                instance.Update();
        }

        void OnDestroy()
        {
            foreach (var instance in _instance.Instances)
                instance.Cleanup();
        }
    }

    public interface IProcessorBase
    {
        void Update();
        void Cleanup();
    }

    public abstract class IComponent<T, T2> : MonoBehaviour where T : IComponent<T, T2>.IProcessor, new() where T2 : IComponent<T, T2>
    {
        static T CachedProcessor;

        protected virtual void Awake()
        {
            CachedProcessor ??= ProcessorsManager.GetInstance<T>();
            if (CachedProcessor.AddOnAwakeRemoveOnDestroy)
                CachedProcessor.Add((T2)this);
        }

        protected virtual void OnDestroy()
        {
            CachedProcessor ??= ProcessorsManager.GetInstance<T>();
            if (CachedProcessor.AddOnAwakeRemoveOnDestroy)
                CachedProcessor.Remove((T2)this);
        }

        protected virtual void OnEnable()
        {
            CachedProcessor ??= ProcessorsManager.GetInstance<T>();
            if (CachedProcessor.AddOnAwakeRemoveOnDestroy == false)
                CachedProcessor.Add((T2)this);
        }

        protected virtual void OnDisable()
        {
            CachedProcessor ??= ProcessorsManager.GetInstance<T>();
            if (CachedProcessor.AddOnAwakeRemoveOnDestroy == false)
                CachedProcessor.Remove((T2)this);
        }

        public interface IProcessor : IProcessorBase
        {
            bool AddOnAwakeRemoveOnDestroy { get; }

            void Add(T2 item);
            void Remove(T2 item);
            void IProcessorBase.Cleanup() => CachedProcessor = default;
        }

        [Serializable]
        public abstract class Processor : IProcessor
        {
            public List<T2> Components = new();
            public bool AddOnAwakeRemoveOnDestroy => false;
            public void Add(T2 item) => Components.Add(item);
            public void Remove(T2 item) => Components.Remove(item);

            public abstract void Update();
        }
    }


    public class CompTest : IComponent<CompTest.ProcTest, CompTest>
    {
        public class ProcTest : Processor
        {
            public override void Update()
            {

            }
        }
    }
}