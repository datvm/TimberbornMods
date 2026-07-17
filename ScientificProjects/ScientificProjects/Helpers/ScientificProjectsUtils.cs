namespace ScientificProjects.Helpers;

public static class ScientificProjectsUtils
{
    public static readonly SingletonKey SaveKey = new("ScientificProjects");

    public const string WorkEffUpgrade1Id = "WorkEffUpgrade1";
    public static readonly FrozenSet<string> WorkEffUpgradeIds = ImmutableHelper.CreateFrozenSet([WorkEffUpgrade1Id, "WorkEffUpgrade2"]);

    public static readonly FrozenSet<string> MoveSpeedUpgradeIds= ImmutableHelper.CreateFrozenSet(["MoveSpeedUp1", "MoveSpeedUp2", "MoveSpeedUp3"]);

    public const string CarryUpgradeId = "CarryUpgrade1";
    public const string CarryBuilderUpgradeId = "CarryBuilderUpgrade";
    public static readonly FrozenSet<string> CarryUpgradeIds = ImmutableHelper.CreateFrozenSet([CarryUpgradeId, CarryBuilderUpgradeId]);

    public const string WheelbarrowsUpgradeId = "Wheelbarrows";

    public const string FtPlankUpgradeId = "FtPlankUpgrade";
    public const string ItSmelterUpgradeId = "ItSmelterUpgrade";

    public static readonly FrozenSet<string> WoodWorkshopTemplateNames = ImmutableHelper.CreateFrozenSet(["WoodWorkshop.Folktails", "WoodWorkshop.IronTeeth"]);
    public static readonly FrozenSet<string> SmelterTemplateNames = ImmutableHelper.CreateFrozenSet(["Smelter.Folktails", "Smelter.IronTeeth"]);

    internal static bool HasFtUpgrade;
    internal static bool HasItUpgrade;

    public static void LogVerbose(Func<string> messageFunc)
        => TimberUiUtils.LogVerbose(() => $"[{nameof(ScientificProjects)}] {messageFunc()}");

    internal static bool HasFtUpgradeEffect(string prefabName) => HasFtUpgrade && WoodWorkshopTemplateNames.Contains(prefabName);
    internal static bool HasItUpgradeEffect(string prefabName) => HasItUpgrade && SmelterTemplateNames.Contains(prefabName);

}
