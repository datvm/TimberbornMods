namespace TimberUi.Services;

public interface IModUpdateNotifier2
{

    string ModId { get; }
    string Version { get; }
    int VersionNumber { get; }
    string MessageLocKey { get; }

}

public class BindModUpdateNotifierAttribute : MultiBindAttribute
{
    
    public BindModUpdateNotifierAttribute() : base(typeof(IModUpdateNotifier2))
    {
        Contexts = BindAttributeContext.MainMenu;
        Scope = Bindito.Core.Internal.Scope.Singleton;
    }

}