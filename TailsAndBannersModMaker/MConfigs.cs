namespace TailsAndBannersModMaker;

[Context(nameof(BindAttributeContext.MainMenu))]
public class MMenuConfigs : MainMenuAttributeConfigurator
{

    public override void Configure()
    {
        base.Configure();

        this.TryBind<UserDecalService>()?.AsSingleton();
        this.TryBind<UserDecalTextureRepository>()?.AsSingleton();
    }

}