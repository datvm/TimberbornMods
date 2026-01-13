namespace BlueprintRelics.Components;

public class BlueprintRelicComponent(
    BlueprintRelicsRegistry registry
) : BaseComponent, IAwakableComponent, IFinishedStateListener, IBlockObjectDeletionBlocker
{

    public bool NoForcedDelete { get; } = false;
    public bool IsStackedDeletionBlocked { get; } = true;
    public bool IsDeletionBlocked { get; } = true;
    public string ReasonLocKey { get; } = "DeletionBlocker.Prefix";

#nullable disable
    public BlueprintRelicSpec Spec { get; private set; }
    DistrictBuilding districtBuilding;
#nullable enable

    public DistrictCenter ConnectedDistrict => districtBuilding.District;

    public void Awake()
    {
        Spec = GetComponent<BlueprintRelicSpec>();
        districtBuilding = GetComponent<DistrictBuilding>();
    }

    public void OnEnterFinishedState() => registry.Register(this);
    public void OnExitFinishedState() => registry.Unregister(this);

}
