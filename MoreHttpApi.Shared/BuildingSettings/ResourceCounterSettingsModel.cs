namespace MoreHttpApi.Shared.BuildingSettings;

public record ResourceCounterSettingsModel(
    HttpResourceCounterMode Mode,
    string? GoodId,
    int Threshold,
    float FillRateThreshold,
    HttpNumericComparisonMode ComparisonMode,
    bool IncludeInputs
);

public enum HttpResourceCounterMode
{
    FillRate,
    StockLevel
}
