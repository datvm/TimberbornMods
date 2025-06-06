namespace ModdableWeather.UI;

public class WeatherDifficultyDescriptor(WeatherDifficulty difficulties) : ModSettingDescriptor("", "")
{
    public WeatherDifficulty Difficutly { get; } = difficulties;
}

public class WeatherDifficultyModSetting(WeatherDifficultyDescriptor descriptor) : NonPersistentSetting(descriptor)
{
    public event EventHandler<WeatherDifficulty>? OnDifficultyRequested;
    public void RequestDifficulty(WeatherDifficulty difficulty) => OnDifficultyRequested?.Invoke(this, difficulty);
}

public class WeatherDifficultyModSettingFactory(ILoc t) : IModSettingElementFactory
{
    static readonly ImmutableArray<WeatherDifficulty> Difficulties = [WeatherDifficulty.Easy, WeatherDifficulty.Normal, WeatherDifficulty.Hard];

    public int Priority { get; }

    public bool TryCreateElement(ModSetting modSetting, out IModSettingElement? element)
    {
        if (modSetting is not WeatherDifficultyModSetting buttons)
        {
            element = default;
            return false;
        }

        var el = CreateButtons(buttons);

        element = new ModSettingElement(el, buttons);
        return true;
    }

    VisualElement CreateButtons(WeatherDifficultyModSetting buttons)
    {
        var row = new VisualElement().SetAsRow();

        foreach (var diff in Difficulties)
        {
            var btn = row.AddMenuButton(
                t.T("NewGameConfigurationPanel." + diff),
                onClick: () => buttons.RequestDifficulty(diff),
                name: "Difficulty" + diff
            )
                .SetMinSize(0, 0)
                .SetFlexGrow(1)
                .SetFlexShrink(1);

            btn.style.flexBasis = 0;
        }

        return row;
    }

}

