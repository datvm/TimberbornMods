global using ModdableToolGroupsHotkeys.Services;
global using ConfigurableToolGroups.UI.CustomBlockObjectProviders;

namespace ModdableToolGroupsHotkeys;

[Context("Game")]
[Context("MapEditor")]
public class MConfigs : Configurator
{
    public override void Configure()
    {
        Bind<KeyBindingEventService>().AsSingleton();
        Bind<ToolHotkeySpecService>().AsSingleton();
        Bind<ToolHotkeyListener>().AsSingleton();
    }
}
