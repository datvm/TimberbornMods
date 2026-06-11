namespace SluiceIsBack.UI;

public class SluiceToggleFactory(SliderToggleFactory sliderToggleFactory, ILoc loc, IAssetLoader assets)
{
    public SluiceToggle Create(VisualElement parent)
    {
        SluiceToggle sluiceToggle = new(sliderToggleFactory, loc, assets);
        sluiceToggle.Initialize(parent);
        return sluiceToggle;
    }
}
