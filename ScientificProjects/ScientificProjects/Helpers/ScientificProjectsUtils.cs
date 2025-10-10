namespace ScientificProjects.Helpers;

public static class ScientificProjectsUtils
{
    public static readonly SingletonKey SaveKey = new("ScientificProjects");

    public const string WorkEffUpgrade1Id = "WorkEffUpgrade1";
    public static readonly FrozenSet<string> WorkEffUpgradeIds = [WorkEffUpgrade1Id, "WorkEffUpgrade2"];

    public static readonly FrozenSet<string> MoveSpeedUpgradeIds= ["MoveSpeedUp1", "MoveSpeedUp2", "MoveSpeedUp3"];

    public const string CarryUpgradeId = "CarryUpgrade1";
    public const string CarryBuilderUpgradeId = "CarryBuilderUpgrade";
    public static readonly FrozenSet<string> CarryUpgradeIds = [CarryUpgradeId, CarryBuilderUpgradeId];

    public const string WheelbarrowsUpgradeId = "Wheelbarrows";

    public static void LogVerbose(Func<string> messageFunc)
        => TimberUiUtils.LogVerbose(() => $"[{nameof(ScientificProjects)}] {messageFunc()}");

}
