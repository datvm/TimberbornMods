global using WaterErosion.Services;
global using WaterErosion.Helpers;

namespace WaterErosion;

public class MWaterErosionConfig : BaseModdableTimberbornAttributeConfiguration
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;
}
