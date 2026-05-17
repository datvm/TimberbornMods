namespace MoreHttpApi.Shared;

public class BuildingSettingsMapper
{

    public static readonly IReadOnlyDictionary<string, Type> BuildingSettingsTypes = new Dictionary<string, Type>
    {
         { "Timberborn.Automation.Automatable", typeof(AutomatableSettingsModel) },
    };

}
