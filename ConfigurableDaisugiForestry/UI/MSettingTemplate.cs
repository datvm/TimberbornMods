namespace ConfigurableDaisugiForestry.UI;

public record MSettingTemplate(
    DaisugiValues Birch,
    DaisugiValues Oak
)
{

    public static readonly MSettingTemplate Easy = new(
        new(4, 1, 1, false),
        new(10, 4, 1, true));

    public static readonly MSettingTemplate Normal = new(
        new(6, 1, 4, false),
        new(14, 4, 16, false)
    );

    public static readonly MSettingTemplate Hard = new(
        new(10, 1, 16, true),
        new(30, 8, 20, true)
    );

}

public record DaisugiValues(int Days, int Logs, int HarvestHours, bool IsPlank);
