
namespace TheArchitectsToolkit.Patches;

#if !TIMBER6

[HarmonyPatch]
public class MapInGamePatches
{
    static readonly ILookup<string, Type> Types = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(assembly => assembly.GetTypes())
        .ToLookup(type => type.FullName, type => type);

    static readonly ImmutableArray<string> AdditionalConfigs =
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
        "Timberborn.SteamWorkshopMapDownloadingUI.SteamWorkshopMapDownloadingUIConfigurator",
        "Timberborn.SteamWorkshopMapUploadingUI.SteamWorkshopMapUploadingUIConfigurator",
        "Timberborn.SteamWorkshopUI.SteamWorkshopUIConfigurator",
    ];

    static readonly HashSet<string> AdditionalServices = [
        "Timberborn.MapEditorUI.FilePanel",
    ];

    [HarmonyPostfix, HarmonyPatch(typeof(ContainerDefinition), nameof(ContainerDefinition.InstallAll))]
    public static void InstallMapEditorConfigs(string contextName, ContainerDefinition __instance)
    {
        if (contextName != "Game" ||
            !MSettings.GameToMap) { return; }

        Unbind(__instance, Types["Timberborn.ThumbnailCapturing.IThumbnailRenderTextureProvider"].Single());

        foreach (var t in AdditionalConfigs)
        {
            Debug.Log($"Installing {t}");

            var type = Types[t].Single();
            var obj = Activator.CreateInstance(type) as IConfigurator;
            __instance.Install(obj);
        }

        foreach (var t in AdditionalServices)
        {
            Debug.Log($"Binding {t}");

            Bind(__instance, Types[t].Single()).AsSingleton();
        }
    }

    static readonly MethodInfo BindMethod = typeof(ContainerDefinition).Method("Bind");
    public static IScopeAssignee Bind(ContainerDefinition containerDefinition, Type type)
    {
        var binding = BindMethod.MakeGenericMethod(type).Invoke(containerDefinition, []) as IScopeAssignee
            ?? throw new ArgumentNullException("binding");

        return binding;
    }

    public static void Unbind(ContainerDefinition containerDefinition, Type type)
    {
        var registry = containerDefinition._bindingBuilderRegistry as BindingBuilderRegistry
            ?? throw new ArgumentNullException("registry");
        registry._boundBindingBuilders.Remove(type);
    }

}

#endif