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
        => new(spec, parent, color, ButtonFactory, T, ButtonService, ToolGroupService, ToolButtonFactory, this);

}
