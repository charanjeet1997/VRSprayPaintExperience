using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Factories
{
    public class FactoryManagerByNameAndType : MonoBehaviour
    {
        public static FactoryManagerByNameAndType Instance { get; private set; }

        private readonly Dictionary<string, object> factoriesByName = new Dictionary<string, object>();
        private readonly Dictionary<Type, object> factoriesByType = new Dictionary<Type, object>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            //factoriesByType = new Dictionary<Type, object>();
            //factoriesByName = new Dictionary<string, object>();
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }


        public static void RegisterFactory<T>(IFactory<T> factory, FactoryConfig<T> config)
        {
            if (Instance == null)
            {
                Debug.LogError("FactoryManager instance not found.");
                return;
            }

            Type factoryType = typeof(T);

            // Ensure uniqueness
            if (Instance.factoriesByName.ContainsKey(config.name))
            {
                Debug.LogError($"Factory for name {config.name} already exists.");
                return;
            }

            if (Instance.factoriesByType.ContainsKey(factoryType))
            {
                Debug.LogError($"Factory for type {factoryType} already exists.");
                return;
            }

            Instance.factoriesByName[config.name] = factory; // Register factory by name
            Instance.factoriesByType[factoryType] = factory; // Register factory by type
            factory.Configure(config, Instance, Instance.transform); // Configure the factory
        }

        public static T CreateByType<T>() where T : MonoBehaviour
        {
            if (Instance.factoriesByType.TryGetValue(typeof(T), out var factory))
            {
                if (factory is IFactory<T> typedFactory)
                {
                    return typedFactory.Create();
                }
            }

            Debug.LogError($"Factory not found for type: {typeof(T)}");
            return default;
        }
        
        // New code
        public static T CreateByType<T>(int prefabIndex) where T : MonoBehaviour
        {
            if (Instance.factoriesByType.TryGetValue(typeof(T), out var factory))
            {
                if (factory is IFactory<T> typedFactory)
                {
                    return typedFactory.Create(prefabIndex);
                }
            }

            Debug.LogError($"Factory not found for type: {typeof(T)}");
            return default;
        }
        
        public static void RecycleByType<T>(T obj, int objIndex) where T : MonoBehaviour
        {
            if (Instance.factoriesByType.TryGetValue(typeof(T), out var factory))
            {
                if (factory is IFactory<T> typedFactory)
                {
                    typedFactory.Recycle(obj, objIndex);
                }
                else
                {
                    Debug.LogError($"Factory not found for type: {typeof(T)}");
                }
            }
            else
            {
                Debug.LogError($"Factory not found for type: {typeof(T)}");
            }
        }
        // New code

        public static void RecycleByType<T>(T obj) where T : MonoBehaviour
        {
            if (Instance.factoriesByType.TryGetValue(typeof(T), out var factory))
            {
                if (factory is IFactory<T> typedFactory)
                {
                    typedFactory.Recycle(obj);
                }
                else
                {
                    Debug.LogError($"Factory not found for type: {typeof(T)}");
                }
            }
            else
            {
                Debug.LogError($"Factory not found for type: {typeof(T)}");
            }
        }

        public static IFactory<T> GetFactoryByType<T>() where T : MonoBehaviour
        {
            if (Instance.factoriesByType.TryGetValue(typeof(T), out var factory))
            {
                if (factory is IFactory<T> typedFactory)
                {
                    return typedFactory;
                }
                else
                {
                    Debug.LogError($"Factory for type '{typeof(T)}' is not of type IFactory<{typeof(T)}>.");
                    return null;
                }
            }
            else
            {
                Debug.LogError($"Factory not found for type: {typeof(T)}");
                return null;
            }
        }

        public static void CleanupFactoryByName<T>(string factoryName)
        {
            if (Instance.factoriesByName.TryGetValue(factoryName, out var factory))
            {
                if (factory is IFactory<T> typedFactory)
                {
                    typedFactory.Cleanup();
                    Instance.factoriesByType.Remove(typeof(T));
                }
                else
                {
                    Debug.LogError($"Factory not found for name: {factoryName}");
                }

                Instance.factoriesByName.Remove(factoryName);
            }
            else
            {
                Debug.LogError($"Factory not found for name: {factoryName}");
            }
        }
        

        public static void CleanupFactoryByType<T>() where T : MonoBehaviour
        {
            Type factoryType = typeof(T);
            if (Instance.factoriesByType.TryGetValue(factoryType, out var factory))
            {
                if (factory is IFactory<T> typedFactory)
                {
                    typedFactory.Cleanup();
                    Instance.factoriesByType.Remove(factoryType);
                }
                else
                {
                    Debug.LogError($"Factory not found for type: {factoryType}");
                }
            }
            else
            {
                Debug.LogError($"Factory not found for type: {factoryType}");
            }
        }

        public static T CreateByName<T>(string factoryName)
        {
            if (Instance.factoriesByName.TryGetValue(factoryName, out var factory))
            {
                if (factory is IFactory<T> typedFactory)
                {
                    return typedFactory.Create();
                }
            }

            Debug.LogError($"Factory not found for name: {factoryName}");
            return default;
        }

        public static void RecycleByName<T>(T obj, string factoryName)
        {
            if (Instance.factoriesByName.TryGetValue(factoryName, out var factory))
            {
                if (factory is IFactory<T> typedFactory)
                {
                    typedFactory.Recycle(obj);
                }
                else
                {
                    Debug.LogError($"Factory not found for name: {factoryName}");
                }
            }
            else
            {
                Debug.LogError($"Factory not found for name: {factoryName}");
            }
        }

        public static IFactory<T> GetFactoryByName<T>(string factoryName)
        {
            if (Instance.factoriesByName.TryGetValue(factoryName, out var factory))
            {
                if (factory is IFactory<T> typedFactory)
                {
                    return typedFactory;
                }
                else
                {
                    Debug.LogError($"Factory for name '{factoryName}' is not of type IFactory<{typeof(T)}>.");
                    return null;
                }
            }
            else
            {
                Debug.LogError($"Factory not found for name: {factoryName}");
                return null;
            }
        }

        public static void CleanupFactoryByType<T>(string name) where T : MonoBehaviour
        {
            Type factoryType = typeof(T);
            Debug.Log("CleanupFactoryByType a");

            if (Instance.factoriesByType.TryGetValue(factoryType, out var factory))
            {
                Debug.Log("CleanupFactoryByType b");

                if (factory is IFactory<T> typedFactory)
                {
                    typedFactory.Cleanup();
                   bool a1= Instance.factoriesByType.Remove(factoryType);
                    bool a2 = Instance.factoriesByName.Remove(name);
                    Debug.Log("Name L "+name);
                    Debug.Log("CleanupFactoryByType C : "+a1+" : a2 : "+a2);

                }
                else
                {
                    Debug.LogError($"Factory not found for type: {factoryType}");
                }
            }
            else
            {
                Debug.LogError($"Factory not found for type: {factoryType}");
            }
        }

        public static void CleanupAllFactories()
        {
            CleanupAllFactoriesByName();
            CleanupAllFactoriesByType();
        }

        private static void CleanupAllFactoriesByName()
        {
            foreach (var factoryPair in Instance.factoriesByName)
            {
                if (factoryPair.Value is IFactory factory)
                {
                    factory.Cleanup();
                }
                else
                {
                    Debug.LogError($"Factory for name: {factoryPair.Key} is not an IFactory");
                }
            }

            Instance.factoriesByName.Clear();
        }

        private static void CleanupAllFactoriesByType()
        {
            foreach (var factoryPair in Instance.factoriesByType)
            {
                if (factoryPair.Value is IFactory factory)
                {
                    factory.Cleanup();
                }
                else
                {
                    Debug.LogError($"Factory for type: {factoryPair.Key} is not an IFactory");
                }
            }

            Instance.factoriesByType.Clear();
        }

    }
}