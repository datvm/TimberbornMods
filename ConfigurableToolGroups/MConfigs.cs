namespace ConfigurableToolGroups;

[Context("MainMenu")]
public class MMenuConfig : Configurator
{
    public override void Configure()
    {
        Bind<TutorialSettingsService>().AsSingleton();
    }
}

[Context("Game")]
[Context("MapEditor")]
public class ConfigurableToolGroupsConfig : Configurator
{

    public override void Configure()
    {
        Bind<TutorialSettingsService>().AsSingleton();

        Bind<ModdableToolGroupButtonFactory>().AsSingleton();

        Bind<ModdableToolGroupSpecService>().AsSingleton();
        Bind<ModdableToolGroupButtonService>().AsSingleton();
        Bind<ToolPanelPositioningService>().AsSingleton();

        MultiBind<BottomBarModule>().To<DummyBottomBarModule>().AsSingleton();
        Bind<ModdableCustomToolButtonService>().AsSingleton();


        this.MultiBindElementsRemover<RemoveOriginalBuiltInElements>();
    }
}

[Context("Game")]
public class MGameConfig : Configurator
{

    public override void Configure()
    {
        this
            .MultiBindCustomTool<BeaverGeneratorButtonCustomRootElement>()
            .MultiBindCustomTool<BotGeneratorButtonCustomRootElement>()
            .MultiBindCustomTool<BuilderPrioritiesButtonCustomRootElement>()
            .MultiBindCustomTool<CursorButtonCustomRootElement>()
            .MultiBindCustomTool<DemolishingButtonCustomRootElement>()
            .MultiBindCustomTool<FieldsButtonCustomRootElement>()
            .MultiBindCustomTool<ForestryButtonCustomRootElement>()
            .MultiBindCustomTool<TreeCuttingAreaButtonCustomRootElement>()            
            .MultiBindCustomTool<ShowOptionsButtonCustomRootElement>()
            .MultiBindCustomTool<WaterHeightBrushButtonCustomRootElement>()

            .MultiBindCustomTool<GameBlockObjectButtonsCustomRootElement>(true)
            .Bind<CustomBlockObjectButtons>().ToExisting<GameBlockObjectButtonsCustomRootElement>()
        ;
    }

}

[Context("MapEditor")]
public class MMapEditorConfig : Configurator
{
    public override void Configure()
    {
        this
            .MultiBindCustomTool<MapEditorToolButtonsCustomRootElement>()
            .MultiBindCustomTool<ShowOptionsButtonCustomRootElement>()

            .MultiBindCustomTool<MapEditorBlockObjectButtonsCustomRootElement>(true)
            .Bind<CustomBlockObjectButtons>().ToExisting<MapEditorBlockObjectButtonsCustomRootElement>()
        ;
    }
}