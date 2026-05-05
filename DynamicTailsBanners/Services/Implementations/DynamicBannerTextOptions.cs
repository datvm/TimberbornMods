namespace DynamicTailsBanners.Services.Implementations;

public class DynamicBannerTextOptions
{
    public string? StatId { get; set; }
    public bool IsCustomText => string.IsNullOrEmpty(StatId);

    public int FontSize
    {
        get => field;
        set
        {
            if (field == value) { return; }

            field = value;
            OnFontSizeChanged?.Invoke(this, value);
        }
    } = 50;
    public event EventHandler<int>? OnFontSizeChanged;

    public SerializableFloats Color { get; set; } = UnityEngine.Color.white;
    public string Content { get; set; } = "Text";

    public PopulationCounterMode PopulationMode { get; set; } = PopulationCounterMode.TotalPopulation;
    public bool CountBeavers { get; set; } = true;
    public bool CountBots { get; set; } = true;

    public PopulationCounterOptions GetPopulationOptions() => new(PopulationMode, CountBeavers, CountBots);
}
