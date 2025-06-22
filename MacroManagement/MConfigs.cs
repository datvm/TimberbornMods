namespace MacroManagement;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<MultiSelectService>()
            .BindSingleton<MacroManagementController>()

            .BindSingleton<UIService>()
            .BindFragment<MultiSelectFragment>()
            .BindFragment<MMFragment>()
        ;
    }
}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        var harmony = new Harmony(nameof(MacroManagement));
        harmony.PatchAll();
    }

}