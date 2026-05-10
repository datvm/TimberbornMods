namespace BuildingSigns.Components;

[AddTemplateModule2(typeof(BuildingSignsSpec))]
public class BuildingSignsComponent : BaseComponent, IAwakableComponent, IStartableComponent, IFinishedStateListener, IPreviewSelectionListener, IDeletableEntity
{

#nullable disable
    BlockObject blockObject;
    public BuildingSignsSpec Spec { get; private set; }
#nullable enable

    public ImmutableArray<BuildingSign> Signs { get; private set; } = [];

    readonly GameObject signsContainer = new("BuildingSigns");

    public void Awake()
    {
        SetSignState(false);

        blockObject = GetComponent<BlockObject>();
        Spec = GetComponent<BuildingSignsSpec>();
    }

    public void Start()
    {
        var t = Transform;

        Signs = [.. Spec.Signs.Select(spec => new BuildingSign(t, blockObject, spec))];
    }

    public void OnEnterFinishedState() => SetSignState(true);
    public void OnExitFinishedState() => SetSignState(false);
    public void OnPreviewSelect() => SetSignState(true);
    public void OnPreviewUnselect() => SetSignState(blockObject.IsFinished);

    void SetSignState(bool show) => signsContainer.SetActive(show);

    public void DeleteEntity()
    {
        foreach (var sign in Signs)
        {
            sign.Dispose();
        }

        Object.Destroy(signsContainer);
    }
}
