global using CopyNameToo.Components;
global using ModdableTimberborn.BuildingSettings;
global using ModdableTimberborn.BuildingSettings.BuiltInSettings;

namespace CopyNameToo;

public class MConfig : BaseModdableTimberbornAttributeConfiguration
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;
}