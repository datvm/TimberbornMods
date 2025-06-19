namespace MacroManagement.Models;

public readonly record struct MacroManagementInfo(
    PrefabSpec PrefabSpec,
    LabeledEntity LabeledEntity,
    ToolButton ToolButton,
    DistrictBuilding? District = null
)
{
    public DistrictCenter? DistrictCenter => District && District.District ? District.District : null;
}

[Flags]
public enum MacroManagementSelectionFlags
{
    None = 0,
    District = 1,
    Running = 2,
    Paused = 4,
}