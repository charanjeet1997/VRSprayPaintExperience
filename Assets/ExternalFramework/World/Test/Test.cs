using Games.WorldSystem;
using ServiceLocatorFramework;
using UnityEngine;

public class Test : MonoBehaviour
{
    public WorldDatabase worldDatabase;
    private IWorldManager worldManager;
    private void Start()
    {
        worldManager = ServiceLocator.Current.Get<IWorldManager>();
    }

    [ContextMenu("CreateWorld")]
    public void CreateWorld()
    {
        // WorldDatabase.WorldData worldData = worldDatabase.GetWorld(WorldName.QuizWorld);
        // worldManager.CreateWorld(worldData);
        worldManager.ChangeWorldStateTo(0f,new WorldInitStateProvider(worldManager.GetCurrentWorldEntity()));
    }
    
    [ContextMenu("DestroyWorld")]
    public void DestroyWorld()
    {
        // worldManager.DestroyWorld(WorldName.QuizWorld);
    }
}
