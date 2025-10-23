namespace WeatherScientificProjects.UI;

public class WeatherForecastPanel(
    UILayout layout,
    ILoc t
) : ILoadableSingleton
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

        container.SetDisplay(false);
    }

    public void SetForecastText(string? text)
    {
        if (text is not null)
        {
            label.text = text;
        }

        container.SetDisplay(text is not null);
    }

    public void SetForecast(TodayForecast? forecast)
    {
        if (forecast is null)
        {
            SetForecastText(null);
        }
        else
        {
            var d = forecast.Value.Duration;
            SetForecastText(string.Format(t.T(d.x == d.y ? "LV.WSP.Forecast1DayText" : "LV.WSP.ForecastText"),
                forecast.Value.Chance,
                forecast.Value.Weather.Spec.Display,
                forecast.Value.Duration.x,
                forecast.Value.Duration.y));
        }
    }

}
