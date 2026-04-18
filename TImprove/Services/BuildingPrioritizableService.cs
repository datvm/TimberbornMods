namespace TImprove.Services;

public class BuildingPrioritizableService(MSettings s) : ILoadableSingleton, IUnloadableSingleton, IPostLoadableSingleton
{
    public static readonly int MaxPriorityValue = (int)Priority.VeryHigh;

    public static BuildingPrioritizableService? Instance { get; private set; }

    public Priority LastSetPriority { get; set; } = Priority.Normal;
    bool remember;
    Priority selectedDefault = Priority.Normal;

    public Priority DefaultBuildingPriority => remember ? LastSetPriority : selectedDefault;

    public void Load()
    {
        Instance = this;
        s.DefaultBuildingPriority.ValueChanged += (_, e) => OnSettingsChanged();
        OnSettingsChanged();
    }

    public void PostLoad()
    {
        LastSetPriority = Priority.Normal;
    }

    void OnSettingsChanged()
    {
        var index = int.Parse(s.DefaultBuildingPriority.Value);
        remember = index >= MaxPriorityValue;

        if (!remember)
        {
            selectedDefault = (Priority)index;
        }
    }

    public void Unload()
    {
        Instance = null;
    }

    
}
