using System;
using System.Collections;
using System.Collections.Generic;
using Game.Factories;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Factories
{
    public class SimpleFactory<T> : IFactory<T> where T : Component
    {
        public Action OnObjectCreated = delegate { };
        public Action OnObjectDestroyed = delegate { };
        
        public string Name
        {
            get
            {
                return _factoryConfig.prefab[0].name;
            }
        }

        protected Queue<T> availableObjects = new Queue<T>();
        // New code
        protected Dictionary<int, Queue<T>> availableObjectsDictionary = new Dictionary<int, Queue<T>>();
        // New code
        [SerializeField] protected List<T> allObjects = new List<T>();
        protected Transform parentTransform;
        protected MonoBehaviour owner;
        protected FactoryConfig<T> _factoryConfig;
        private Coroutine creationRoutine;
        
        public int PrefabCount => _factoryConfig.prefab.Count;
        
        public void Configure(FactoryConfig<T> config, MonoBehaviour factoryManager, Transform parent = null)
        {
            this.owner = factoryManager;
            this.parentTransform = parent;
            this._factoryConfig = config;
            if (config.startImmediately)
            {
                StartCreation();
            }
        }

        private void StartCreation()
        {
            creationRoutine = owner.StartCoroutine(CreateObjectsOverTime(_factoryConfig.numberOfObjectsToCreate, _factoryConfig.delayBetweenInstances));
        }
        
        public virtual T AddObject()
        {
            var newObject = Object.Instantiate(_factoryConfig.prefab[0], parentTransform);
            newObject.gameObject.SetActive(false);
            availableObjects.Enqueue(newObject);
            allObjects.Add(newObject);
            return newObject;
        }
        
        public virtual T Create()
        {
            if (availableObjects.Count == 0)
            {
                AddObject();
            }

            var obj = availableObjects.Dequeue();
            if (obj is IManagedLifecycle<T> managedLifecycle)
            {
                managedLifecycle.Activate();
            }
            
            obj.gameObject.SetActive(true);
            return obj;
        }

        // New code
        public virtual T Create(int prefabIndex = 0)
        {
            if (!availableObjectsDictionary.TryGetValue(prefabIndex, out var queue))
            {
                queue = new Queue<T>();
                availableObjectsDictionary[prefabIndex] = queue;
            }

            T obj = null;
            if (queue.Count == 0)
            {
                 obj = AddObject(prefabIndex);
            }

            obj = queue.Dequeue();
            if (obj == null)
            {
                obj = AddObject(prefabIndex);
            }
            obj.gameObject.SetActive(true);

            if (obj is IManagedLifecycle<T> managedLifecycle)
            {
                managedLifecycle.Activate();
            }

            return obj;
        }
        
        public virtual T AddObject(int prefabIndex = 0)
        {
            // Debug.Log($"Prefab Index: {prefabIndex}");
            var newObject = Object.Instantiate(_factoryConfig.prefab[prefabIndex], parentTransform);
            AddToPool(newObject, prefabIndex);
            return newObject;
        }

        protected void AddToPool(T obj, int prefabIndex)
        {
            obj.gameObject.SetActive(false);

            if (!availableObjectsDictionary.ContainsKey(prefabIndex))
            {
                availableObjectsDictionary[prefabIndex] = new Queue<T>();
            }

            availableObjectsDictionary[prefabIndex].Enqueue(obj);
            allObjects.Add(obj);
        }

        public void Recycle(T obj, int objIndex)
        {
            if (obj == null) return;

            if (obj is IManagedLifecycle<T> managedLifecycle)
            {
                managedLifecycle.Deactivate();
                managedLifecycle.Initialize();
            }

            obj.gameObject.SetActive(false);
            obj.transform.parent = parentTransform;

            // Ensure a queue exists for the prefabIndex in the dictionary
            if (!availableObjectsDictionary.ContainsKey(objIndex))
            {
                availableObjectsDictionary[objIndex] = new Queue<T>();
            }

            // Add the object back to the appropriate queue
            availableObjectsDictionary[objIndex].Enqueue(obj);

            Debug.Log($"Object recycled to prefabIndex {objIndex} pool.");
        }
        // New code


        public void Recycle(T obj)
        {
            if(obj == null) return;

            if (obj is IManagedLifecycle<T> managedLifecycle)
            {
                managedLifecycle.Deactivate();
                managedLifecycle.Initialize();
            }

            obj.gameObject.SetActive(false);
            obj.transform.parent = parentTransform;
            availableObjects.Enqueue(obj);
        }

        public virtual void Cleanup()
        {
            foreach (var tempObject in allObjects)
            {
                if(tempObject!=null) 
                    Object.Destroy(tempObject.gameObject);                
            }
            
            allObjects.Clear();
            availableObjects.Clear();
            OnObjectDestroyed();
        }
        
        public IEnumerator CreateObjectsOverTime(int numberOfObjects, float delayBetweenInstances)
        {
            for (int i = 0; i < numberOfObjects; i++)
            {
                AddObject();
                yield return new WaitForSeconds(delayBetweenInstances);
            }

            OnObjectCreated();
        }

        private void StopCreationRoutine()
        {
            if(creationRoutine!=null)
            {
                owner.StopCoroutine(creationRoutine);
            }
        }
    }
}