namespace ModdableTimberborn;

[Context(nameof(ConfigurationContext.Bootstrapper))]
public class ModBootstrapperConfig : Configurator
{
    public override void Configure()
    {
        ModdableTimberbornRegistry.Instance.Configure(this, ConfigurationContext.Bootstrapper);
    }
}

[Context(nameof(ConfigurationContext.MainMenu))]
public class ModMainMenuConfig : Configurator
{
    public override void Configure()
    {
        ModdableTimberbornRegistry.Instance.Configure(this, ConfigurationContext.MainMenu);
    }
}

[Context(nameof(ConfigurationContext.Game))]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        ModdableTimberbornRegistry.Instance.Configure(this, ConfigurationContext.Game);
    }
}

[Context(nameof(ConfigurationContext.MapEditor))]
public class ModMapEditorConfig : Configurator
{
    public override void Configure()
    {
        ModdableTimberbornRegistry.Instance.Configure(this, ConfigurationContext.MapEditor);
    }
}

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment) => ModdableTimberbornRegistry.Instance.ConfigureStarter();

}
