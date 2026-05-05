namespace TimberUi;

[Context(nameof(BindAttributeContext.Bootstrapper))]
public class MBootstrapperConfig : BootstrapperAttributeConfigurator { }

[Context(nameof(BindAttributeContext.MainMenu))]
public class MMenuConfig : MainMenuAttributeConfigurator { }

[Context(nameof(BindAttributeContext.Game))]
public class MGameConfig : GameAttributeConfigurator { }

[Context(nameof(BindAttributeContext.MapEditor))]
public class MMapConfig : MapEditorAttributeConfigurator { }