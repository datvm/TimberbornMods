namespace ModdableTimberbornDemo.Features.DI;

public class DemoPrefabTailRunner : IPrefabGroupServiceTailRunner, ILoadableSingleton
{
    public void Load()
    {
        Debug.Log("DemoPrefabTailRunner.Load is called. This should be before PrefabGroupService.Load");
    }

    public void Run(PrefabGroupService prefabGroupService)
    {
        Debug.Log("DemoPrefabTailRunner.Run is running!");
    }
}
