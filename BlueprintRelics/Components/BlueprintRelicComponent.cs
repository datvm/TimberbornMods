namespace BlueprintRelics.Components;

[AddTemplateModule2(typeof(BlueprintRelicSpec))]
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
#nullable enable

    public void Awake()
    {
        Spec = GetComponent<BlueprintRelicSpec>();
    }

    public void OnEnterFinishedState() => registry.Register(this);
    public void OnExitFinishedState() => registry.Unregister(this);

}
