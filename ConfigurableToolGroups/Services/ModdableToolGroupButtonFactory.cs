namespace ConfigurableToolGroups.Services;

public class ModdableToolGroupButtonFactory(
    ToolGroupButtonFactory ButtonFactory,
    ILoc T,
    ModdableToolGroupButtonService ButtonService,
    ToolGroupService ToolGroupService,
    ToolButtonFactory ToolButtonFactory
)
{

    public ModdableToolGroupButton Create(ToolGroupSpec spec, ModdableToolGroupButton? parent, ToolButtonColor color = default)
        => Create(spec, parent, color, isRoot: parent is null);

    public ModdableToolGroupButton Create(ToolGroupSpec spec, ModdableToolGroupButton? parent, ToolButtonColor color = default, bool isRoot = false)
        => new(spec, parent, color, ButtonFactory, T, ButtonService, ToolGroupService, ToolButtonFactory, this, isRoot);

}
