namespace TimberUi;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class BindAttribute : Attribute
{

    public BindAttributeContext Contexts { get; init; } = BindAttributeContext.Game;
    public Scope? Scope { get; init; }
    public Type? As { get; init; }
    public bool AlsoBindSelf { get; init; }

    public bool MultiBind { get; init; }

}

[Flags]
public enum BindAttributeContext
{
    None = 0,
    MainMenu = 1,
    Game = 2,
    MapEditor = 4,
    Bootstrapper = 8,
    All = MainMenu | Game | MapEditor,
    NonMenu = Game | MapEditor,
}