namespace DecorativePlants;

[Context(nameof(BindAttributeContext.MainMenu))]
public class MMenuConfig : MainMenuAttributeConfigurator
{

    public override void Configure()
    {
        base.Configure();

        this.TryBindingSpriteOperations();
    }

}

[Context(nameof(BindAttributeContext.Game))]
public class MGameConfig : GameAttributeConfigurator;