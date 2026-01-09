namespace TimberUi.Services;

public class MinMaxSliderInitializer(IAssetLoader assets) : IVisualElementInitializer, ILoadableSingleton
{

#nullable disable
    Texture2D sliderHolder, sliderHolderHover, sliderBar;
#nullable enable

    public void Load()
    {
        sliderHolder = assets.Load<Texture2D>("UI/Images/Buttons/slider_holder");
        sliderHolderHover = assets.Load<Texture2D>("UI/Images/Buttons/slider_holder_hover");
        sliderBar = assets.Load<Texture2D>("UI/Images/Buttons/slider_bar");
    }

    public void InitializeVisualElement(VisualElement el)
    {
        if (el is not MinMaxSlider slider) { return; }

        var thumbs = slider.Q(name: "unity-dragger").Children().Where(q => q.name.StartsWith("unity-thumb"));
        foreach (var t in thumbs)
        {
            t.SetSize(25).SetMargin(top: -10);
            t.style.backgroundImage = new StyleBackground(sliderHolder);

            t.RegisterCallback<MouseOverEvent>(_ => t.style.backgroundImage = new StyleBackground(sliderHolderHover));
            t.RegisterCallback<MouseOutEvent>(_ => t.style.backgroundImage = new StyleBackground(sliderHolder));
        }

        var trackerBar = slider.Q(name: "unity-tracker");
        trackerBar.SetHeight(4);
        trackerBar.style.backgroundImage = new StyleBackground(sliderBar);
        trackerBar.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Cover);
    }

    
}
