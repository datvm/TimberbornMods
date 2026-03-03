namespace SluiceIsBack.UI;

public class SluiceToggleFactory(SliderToggleFactory sliderToggleFactory, ILoc loc)
{
    public SluiceToggle Create(VisualElement parent)
    {
        SluiceToggle sluiceToggle = new(sliderToggleFactory, loc);
        sluiceToggle.Initialize(parent);
        return sluiceToggle;
    }
}
