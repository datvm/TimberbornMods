namespace TheArchitectsToolkit.Patches;

#if TIMBER6

[HarmonyPatch]
public static class MapInGamePatches
{

    static readonly ILookup<string, Type> Types = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(assembly => assembly.GetTypes())
        .ToLookup(type => type.FullName, type => type);

    static readonly HashSet<string> AdditionalConfigs =
    [
        "Timberborn.FileBrowsing.FileBrowsingConfigurator",
        "Timberborn.MapEditorPersistence.MapEditorPersistenceConfigurator",
        "Timberborn.MapEditorPersistenceUI.MapEditorPersistenceUIConfigurator",
        "Timberborn.MapEditorSceneLoading.MapEditorSceneLoadingConfigurator",
        "Timberborn.MapMetadataSystemUI.MapMetadataSystemUIConfigurator",
        "Timberborn.MapRepositorySystemUI.MapRepositorySystemUIConfigurator",
        "Timberborn.MapThumbnailCapturing.MapThumbnailCapturingConfigurator",
        "Timberborn.MapThumbnailCapturingUI.MapThumbnailCapturingUIConfigurator",
        "Timberborn.MapThumbnailOverlaySystem.MapThumbnailOverlaySystemConfigurator",
        "Timberborn.RuinsModelShuffling.RuinsModelShufflingConfigurator",
        "Timberborn.SettingsSystem.SettingsSystemConfigurator",
        "Timberborn.SteamWorkshopMapDownloadingUI.SteamWorkshopMapDownloadingUIConfigurator",
        "Timberborn.SteamWorkshopMapUploadingUI.SteamWorkshopMapUploadingUIConfigurator",
        "Timberborn.SteamWorkshopUI.SteamWorkshopUIConfigurator",
    ];

    static readonly HashSet<string> AdditionalServices = [
        "Timberborn.MapEditorUI.FilePanel",
    ];

    [HarmonyPostfix, HarmonyPatch("Timberborn.GameScene.GameSceneInstaller", "Configure")]
    public static void AfterConfigure(IContainerDefinition containerDefinition)
    {
        if (!MSettings.GameToMap) { return; }

        Unbind(containerDefinition, Types["Timberborn.ThumbnailCapturing.IThumbnailRenderTextureProvider"].Single());

        foreach (var t in AdditionalConfigs)
        {
            Debug.Log(t);
            var type = Types[t].Single();
            var obj = Activator.CreateInstance(type) as IConfigurator;

            containerDefinition.Install(obj);
        }

        var bindMethod = containerDefinition.GetType().Method("Bind");
        foreach (var t in AdditionalServices)
        {
            Debug.Log(t);
            var type = Types[t].Single();

            var binding = bindMethod.MakeGenericMethod(type).Invoke(containerDefinition, []) as IScopeAssignee
                ?? throw new ArgumentNullException("binding");
            binding.AsSingleton();            
        }

        containerDefinition.Bind<ToolkitGameService>().AsSingleton();
    }

    static readonly FieldInfo bindingRegistry = typeof(ContainerDefinition).Field("_bindingBuilderRegistry");
    static readonly FieldInfo boundField = typeof(BindingBuilderRegistry).Field("_boundBindingBuilders");
    public static void Unbind(IContainerDefinition containerDefinition, Type type)
    {
        var registry = bindingRegistry.GetValue(containerDefinition) as BindingBuilderRegistry
            ?? throw new ArgumentNullException("registry");
        var bound = boundField.GetValue(registry) as Dictionary<Type, IBindingBuilder>
            ?? throw new ArgumentNullException("bound");

        bound.Remove(type);
    }



}

#endif