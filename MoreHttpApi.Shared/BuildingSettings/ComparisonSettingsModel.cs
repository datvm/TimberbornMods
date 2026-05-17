namespace MoreHttpApi.Shared.BuildingSettings;

public record ComparisonSettingsModel(HttpNumericComparisonMode Mode, float Threshold)
{
    public override string ToString() => ToString("F1");
    public string ToString(string format) => $"{Mode.ToChar()}{Threshold.ToString(format)}";
}

public enum HttpNumericComparisonMode
{
    Greater,
    Less,
    GreaterOrEqual,
    LessOrEqual,
    Equal,
    NotEqual
}
