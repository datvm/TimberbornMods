namespace MoreHttpApi;

[Context(nameof(BindAttributeContext.MainMenu))]
public class MMainMenuConfig : MainMenuAttributeConfigurator;

[Context(nameof(BindAttributeContext.Game))]
public class MGameConfig : GameAttributeConfigurator;

[Context(nameof(BindAttributeContext.MapEditor))]
public class MMapEditorConfig : MapEditorAttributeConfigurator;