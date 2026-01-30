namespace ConfigurableFaction.Models;

public abstract class TemplateDefBase : IIdDef
{

    public Blueprint Blueprint { get; }
    public string BlueprintPath { get; }

    public TemplateSpec Spec { get; }
    public string TemplateName => Spec.TemplateName;
    public string Id => BlueprintPath;
    public string DisplayName { get; }
    public Sprite Sprite { get; }

    public abstract int Order { get; }
    public abstract string? PlanterGroup { get; }

    public FrozenSet<GoodDef> RequiredGoods { get; protected set; } = [];
    public FrozenSet<string> RequiredNeeds { get; protected set; } = [];

    public TemplateDefBase(Blueprint Blueprint, string BlueprintPath, DataAggregatorService dataAggregator, ILoc t)
    {
        this.Blueprint = Blueprint;
        this.BlueprintPath = BlueprintPath;

        Spec = Blueprint.GetSpec<TemplateSpec>();

        var label = Blueprint.GetSpec<LabeledEntitySpec>();
        DisplayName = t.T(label.DisplayNameLocKey);
        Sprite = label.Icon.Asset;

        InitializeRequirements(dataAggregator);
    }


    protected abstract void InitializeRequirements(DataAggregatorService dataAggregator);

}
