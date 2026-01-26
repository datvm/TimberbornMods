namespace ConfigurableFaction.Definitions;

public abstract class TemplateDefBase
{

    public Blueprint Blueprint { get; }

    public TemplateSpec Spec { get; }
    public string TemplateName => Spec.TemplateName;
    public string DisplayName { get; }

    public FrozenSet<GoodDef> RequiredGoods { get; protected set; } = [];
    public FrozenSet<string> RequiredNeeds { get; protected set; } = [];

    public TemplateDefBase(Blueprint Blueprint, DataAggregatorService dataAggregator, ILoc t)
    {
        this.Blueprint = Blueprint;
        Spec = Blueprint.GetSpec<TemplateSpec>();
        DisplayName = t.T(Blueprint.GetSpec<LabeledEntitySpec>().DisplayNameLocKey);

        InitializeRequirements(dataAggregator);
    }

    protected abstract void InitializeRequirements(DataAggregatorService dataAggregator);

}
