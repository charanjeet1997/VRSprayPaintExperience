using System.Collections;
using UnityEngine;

namespace Game.Factories
{
    public interface IFactory
    {
        string Name { get; }
        void Cleanup();
    }

    public interface IFactory<T> : IFactory
    {
        T Create();
        T Create(int prefabIndex); // New code
        void Recycle(T obj);
        void Recycle(T obj, int objIndex); // New code
        void Configure(FactoryConfig<T> config, MonoBehaviour factoryManager, Transform parent = null);
    }
}
