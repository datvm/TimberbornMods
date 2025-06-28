namespace HydroFormaProjects.Helpers;

public static class HydroFormaModUtils
{
    public const string DamUpgrade = "HFDamUpgrade";
    public const string LeveeUpgrade = "HFLeveeUpgrade";
    public const string FloodgateUpgrade = "HFFloodgateUpgrade";
    public const string SluiceUpgrade = "HFSluiceUpgrade";
    public const string ImpermeableFloorUpgrade = "HFImpermeableFloorUpgrade";
    public const string DirtExcavatorUpgrade = "HFDirtExcavatorUpgrade";
    public const string TerrainBlockUpgrade1 = "HFTerrainBlockUpgrade1";
    public const string TerrainBlockUpgrade2 = "HFTerrainBlockUpgrade2";
    public const string BarrierUpgrade = "HFBarrierUpgrade";
    public const string StreamGaugeUpgrade = "HFStreamGaugeUpgrade";

    public static readonly ImmutableHashSet<string> TerrainBlockUpgrades = [TerrainBlockUpgrade1, TerrainBlockUpgrade2];
}