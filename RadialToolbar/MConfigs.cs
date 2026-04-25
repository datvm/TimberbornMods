namespace RadialToolbar;

[Context(nameof(BindAttributeContext.Game))]
public class MGameConfigs : GameAttributeConfigurator;

[Context(nameof(BindAttributeContext.MainMenu))]
public class MMenuConfigs : MainMenuAttributeConfigurator;

[Context(nameof(BindAttributeContext.MapEditor))]
public class MMapEditorConfigs : MapEditorAttributeConfigurator;