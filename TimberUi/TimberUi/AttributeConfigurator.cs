namespace TimberUi;

public abstract class AttributeConfigurator : Configurator
{
    public abstract BindAttributeContext Context { get; }
    public virtual Assembly GetAssembly() => GetType().Assembly;

    public override void Configure() => this.BindAttributes(Context, assembly: GetAssembly());
}

public class MainMenuAttributeConfigurator : AttributeConfigurator
{
    public override BindAttributeContext Context { get; } = BindAttributeContext.MainMenu;
}

public class GameAttributeConfigurator : AttributeConfigurator
{
    public override BindAttributeContext Context { get; } = BindAttributeContext.Game;
}

public class MapEditorAttributeConfigurator : AttributeConfigurator
{
    public override BindAttributeContext Context { get; } = BindAttributeContext.MapEditor;
}

public class BootstrapperAttributeConfigurator : AttributeConfigurator
{
    public override BindAttributeContext Context { get; } = BindAttributeContext.Bootstrapper;
}