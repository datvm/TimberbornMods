global using TimberApi.UIBuilderSystem;
global using TimberApi.UIBuilderSystem.ElementBuilders;
global using Timberborn.CoreUI;
using TimberApi.UIPresets.Labels;
using TimberApi.UIPresets.Sliders;

namespace LateGamePower.UI;

public class PowerPanelFragment : PowerPanel<PowerPanelFragment>
{
    protected override PowerPanelFragment BuilderInstance => this;
}

public abstract class PowerPanel<TBuilder> : BaseBuilder<TBuilder, NineSliceVisualElement>
    where TBuilder : BaseBuilder<TBuilder, NineSliceVisualElement>
{
    PanelFragment? builder = null!;
    ILoc loc = null!;

    [Inject]
    public void InjectDependencies(ILoc loc)
    {
        this.loc = loc;
    }

    static readonly string[] LabelsFirstSet = ["ScienceOverdrive", "CurrentMul", "NoScience", "SetMul" ];
    static readonly string[] LabelsSecondSet = ["PrevCost", "NextCost"];

    protected override NineSliceVisualElement InitializeRoot()
    {
        builder = UIBuilder.Create<PanelFragment>();

        foreach (var l in LabelsFirstSet)
        {
            builder.AddComponent(UIBuilder
                .Create<GameTextLabel>(l)
                .SetText(loc.T("LV.LGP." + l))
                .Build());
        }

        builder.AddComponent(UIBuilder
            .Create<GameTextSliderInt>("TargetMul")
                .SetLowValue(1)
                .SetHighValue(10)
                .Build());

        foreach (var l in LabelsSecondSet)
        {
            builder.AddComponent(UIBuilder
                .Create<GameTextLabel>(l)
                .SetText(loc.T("LV.LGP." + l))
                .Build());
        }

        builder.SetFlexDirection(FlexDirection.Column);
        builder.SetWidth(new Length(100f, LengthUnit.Percent));
        builder.SetJustifyContent(Justify.SpaceBetween);
        return builder.BuildAndInitialize();
    }

}
