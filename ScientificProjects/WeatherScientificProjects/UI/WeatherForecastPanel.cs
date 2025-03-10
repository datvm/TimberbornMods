namespace WeatherScientificProjects.UI;

public class WeatherForecastPanel(UILayout layout) : ILoadableSingleton
{
    VisualElement container = null!;
    Label label = null!;

    public void Load()
    {
        container = new NineSliceButton()
            .AddClasses([UiCssClasses.TopRightItemClass, UiCssClasses.ButtonTopBarPrefix + UiCssClasses.Green]);
        container.style.justifyContent = Justify.Center;

        label = container.AddGameLabel(color: UiBuilder.GameLabelColor.Yellow);

        layout.AddTopRight(container, 7);

        container.ToggleDisplayStyle(false);
    }

    public void SetForecastText(string? text)
    {
        if (text is not null)
        {
            label.text = text;
        }

        container.ToggleDisplayStyle(text is not null);
    }

}
