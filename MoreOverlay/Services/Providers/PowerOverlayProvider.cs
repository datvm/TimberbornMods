namespace MoreOverlay.Services.Providers;

[MultiBind(typeof(IMoreOverlayProvider))]
public class PowerOverlayProvider(NamedIconProvider namedIconProvider, MoreOverlayHighlighter highlighter) : ComponentOverlayProviderBase<MechanicalNode, PowerOverlayInstance>
{
    protected override PowerOverlayInstance? CreateInstance(MoreOverlayComponent overlayComp, MechanicalNode comp)
        => (comp.IsConsumer || comp.IsGenerator || comp.IsBattery)
            ? new(overlayComp, comp, namedIconProvider, highlighter)
            : null;
}

public class PowerOverlayInstance(MoreOverlayComponent overlayComp, MechanicalNode comp, NamedIconProvider namedIconProvider, MoreOverlayHighlighter highlighter)
    : ComponentOverlayInstanceBase<MechanicalNode>(overlayComp, comp), ITickableMoreOverlayInstance
{

#nullable disable
    IconSpan power;
#nullable enable

    bool isGenerator;
    bool isBattery;
    IconSpan? network;

    public override void Initialize(VisualElement container)
    {
        base.Initialize(container);

        power = el.AddIconSpan(namedIconProvider.GetOrLoadGameIcon("ico-power", "ico-power"), size: IconSize).SetMarginRight(5);

        isGenerator = Component.IsGenerator;
        isBattery = Component.IsBattery;
        if (isGenerator || isBattery)
        {
            network = container
                .AddIconSpan(namedIconProvider.GetOrLoad("buildingroups-power", "Sprites/BottomBar/BuildingGroups/Power"), size: IconSize)
                .SetMarginBottom(5);
        }
    }

    public void OnTickUpdate() => UpdateData();

    public override void UpdateData()
    {
        var actual = Component.Actuals;

        var text = isGenerator ? actual.PowerOutput.ToString()
            : isBattery ? $"{actual.BatteryCharge}/{actual.BatteryCapacity}"
            : $"{Mathf.RoundToInt(actual.PowerInput * Component.PowerEfficiency)}/{actual.PowerInput}";
        power.SetPostfixText(text);

        if (isGenerator)
        {
            var graph = Component.Graph;
            var eff = graph.PowerEfficiency;
            network!.SetPostfixText($"{graph.PowerSupply}/{graph.PowerDemand} ({eff:P0})");

            highlighter.Highlight(OverlayComponent, eff, 90);
        }
        else if (isBattery)
        {
            var graph = Component.Graph;
            network!.SetPostfixText($"{graph.BatteryCharge}/{graph.BatteryCapacity}");

            highlighter.Highlight(OverlayComponent, (float)actual.BatteryCharge / actual.BatteryCapacity, 90);
        }
    }
}
