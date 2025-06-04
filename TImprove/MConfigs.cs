namespace TImprove;

public class DefaultModConfigurator : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
        Bind<MSettingsNewDay>().AsSingleton();

        Bind<QuickQuitService>().AsSingleton();
    }
}

public class NonMenuConfigurator : DefaultModConfigurator
{

    public override void Configure()
    {
        base.Configure();

        Bind<CoordsPanel>().AsSingleton();
        Bind<CoordsService>().AsSingleton();
    }

}

[Context("MainMenu")]
public class ModMenuConfig : DefaultModConfigurator
{
    public override void Configure()
    {
        base.Configure();
    }
}

[Context("Game")]
public class GameConfig : NonMenuConfigurator
{
    public override void Configure()
    {
        base.Configure();

        Bind<TimeService>().AsSingleton();
        Bind<GameDepServices>().AsSingleton();
        Bind<RealTimeComponent>().AsSingleton();
        Bind<NewDayService>().AsSingleton();
    }
}

[Context("MapEditor")]
public class MapEditorConfig : NonMenuConfigurator
{
    public override void Configure()
    {
        base.Configure();
    }
}


public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(TImprove)).PatchAll();
    }

}
