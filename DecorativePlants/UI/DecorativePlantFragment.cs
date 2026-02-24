namespace DecorativePlants.UI;

[BindFragment]
public class DecorativePlantFragment(
    ILoc t
) : BaseEntityPanelFragment<DecorativePlantComponent>
{

    Toggle[] chkMature = [], chkWellness = [];
    Toggle chkHasProduct = null!;

    protected override void InitializePanel()
    {
        chkMature = CreateOptions(panel, DecorativePlantComponent.AllMatureStates, "LV.DP.PlantMature", OnMatureSelected);
        chkWellness = CreateOptions(panel, DecorativePlantComponent.AllWellnessStates, "LV.DP.PlantWellness", OnWellnessSelected);

        chkHasProduct = panel.AddToggle(t.T("LV.DP.HasProduct"), onValueChanged: OnHasProductChanged);
    }

    Toggle[] CreateOptions<T>(VisualElement parent, ImmutableArray<T> values, string keyPrefix, Action<T> onSelected)
    {
        var row = parent.AddRow().SetMarginBottom(5);
        var result = new Toggle[values.Length];

        for (int i = 0; i < values.Length; i++)
        {
            Toggle chk = null!;
            var value = values[i]!;

            chk = result[i] = row.AddToggle(t.T(keyPrefix + value.ToString()), onValueChanged: v =>
            {
                if (v) { onSelected(value); }

                UpdateChecks();
            });
        }

        return result;
    }

    void OnMatureSelected(PlantMatureState state) => component!.SetState(mature: state);
    void OnWellnessSelected(PlantWellnessState state) => component!.SetState(wellness: state);
    void OnHasProductChanged(bool hasProduct) => component!.SetState(hasProduct: hasProduct);

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);

        UpdateChecks();
    }

    void UpdateChecks()
    {
        if (!component) { return; }

        var mature = (int)component!.MatureState;
        var wellness = (int)component!.WellnessState;

        for (int i = 0; i < chkMature.Length; i++)
        {
            chkMature[i].SetValueWithoutNotify(i == mature);
        }

        for (int i = 0; i < chkWellness.Length; i++)
        {
            chkWellness[i].SetValueWithoutNotify(i == wellness);
        }

        chkHasProduct.SetValueWithoutNotify(component!.YieldState);
        chkHasProduct.enabledSelf = mature == (int)PlantMatureState.Mature && wellness != (int)PlantWellnessState.Dead;
    }

}
