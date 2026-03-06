namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record ComparisonSettingsModel(NumericComparisonMode Mode, float Threshold)
{
    public override string ToString() => ToString("F1");
    public string ToString(string format) => $"{Mode.ToChar()}{Threshold.ToString(format)}";
}
