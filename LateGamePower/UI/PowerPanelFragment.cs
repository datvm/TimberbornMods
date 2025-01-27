global using TimberApi.UIBuilderSystem;
global using Timberborn.CoreUI;
global using TimberApi.UIBuilderSystem.ElementBuilders;
using TimberApi.UIPresets.Sliders;
using TimberApi.UIPresets.Labels;

namespace LateGamePower.UI;

public class PowerPanelFragment : PowerPanel<PowerPanelFragment>
{
    protected override PowerPanelFragment BuilderInstance => this;
}

public abstract class PowerPanel<TBuilder> : BaseBuilder<TBuilder, NineSliceVisualElement>
    where TBuilder : BaseBuilder<TBuilder, NineSliceVisualElement>
{
    PanelFragment? builder = null!;

    protected override NineSliceVisualElement InitializeRoot()
    {
        builder = UIBuilder.Create<PanelFragment>();

        builder.AddComponent(UIBuilder
            .Create<GameTextLabel>("PowerMultiplicationDesc")
            .SetText("x2 Power, -3/h")
            .Build());

        builder.AddComponent(UIBuilder
            .Create<GameTextSliderInt>()
                .SetName("PowerMultiplication")
                .SetLowValue(1)
                .SetHighValue(10)
                .SetLabel("Supercharge")
                .Build());

        builder.SetFlexDirection(FlexDirection.Column);
        builder.SetWidth(new Length(100f, LengthUnit.Percent));
        builder.SetJustifyContent(Justify.SpaceBetween);
        return builder.BuildAndInitialize();
    }

}
