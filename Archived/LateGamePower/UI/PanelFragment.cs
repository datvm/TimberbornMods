global using TimberApi.UIBuilderSystem.StyleSheetSystem;
global using TimberApi.UIBuilderSystem.StylingElements;
global using UnityEngine.UIElements;

namespace LateGamePower.UI;

public class PanelFragment : PanelFragment<PanelFragment>
{
    protected override PanelFragment BuilderInstance => this;
}

public abstract class PanelFragment<TBuilder> : BaseBuilder<TBuilder, NineSliceVisualElement> where TBuilder : BaseBuilder<TBuilder, NineSliceVisualElement>
{
    const string BackgroundClass = "PanelFragment";

    VisualElementBuilder visualElementBuilder = null!;

    protected override NineSliceVisualElement InitializeRoot()
    {
        visualElementBuilder = UIBuilder.Create<VisualElementBuilder>();
        visualElementBuilder.AddClass(BackgroundClass);
        visualElementBuilder.SetPadding(new Padding(new Length(12f, LengthUnit.Pixel), new Length(8f, LengthUnit.Pixel)));
        return visualElementBuilder.Build();
    }

    public TBuilder AddComponent(Type builderType)
    {
        base.Root.Add(UIBuilder.Build(builderType));
        return BuilderInstance;
    }

    public TBuilder AddComponent(VisualElement visualElement)
    {
        base.Root.Add(visualElement);
        return BuilderInstance;
    }

    public TBuilder SetFlexDirection(FlexDirection direction)
    {
        base.Root.style.flexDirection = direction;
        return BuilderInstance;
    }

    public TBuilder SetWidth(Length width)
    {
        base.Root.style.width = width;
        return BuilderInstance;
    }

    public TBuilder SetJustifyContent(Justify justify)
    {
        base.Root.style.justifyContent = justify;
        return BuilderInstance;
    }

    protected override void InitializeStyleSheet(StyleSheetBuilder styleSheetBuilder)
    {
        styleSheetBuilder.AddNineSlicedBackgroundClass("PanelFragment", "ui/images/backgrounds/bg-3", 9f, 0.5f);
    }
}
