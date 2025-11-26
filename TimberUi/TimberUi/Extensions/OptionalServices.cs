namespace Bindito.Core;

public static partial class UiBuilderExtensions
{

    [Obsolete($"Use {nameof(TryBindingCameraShake)} instead.")]
    public static Configurator TryAddingCameraShake(this Configurator configurator, bool isMenuContext)
        => TryBindingCameraShake(configurator, isMenuContext);

    /// <summary>
    /// Bind services needed for camera shake
    /// </summary>
    /// <remarks>
    /// This method can be called multiple times in case it's called from multiple mods.
    /// </remarks>
    public static Configurator TryBindingCameraShake(this Configurator configurator, bool isMenuContext)
    {
        if (!isMenuContext)
        {
            configurator.TryBind<CameraShakeService>()?.AsSingleton();
        }
        configurator.TryBind<CameraShakeSettingService>()?.AsSingleton();

        return configurator;
    }

    /// <summary>
    /// Bind services needed for loading AudioClip (WAV) files from mods' file system.
    /// </summary>
    /// <remarks>
    /// This method can be called multiple times in case it's called from multiple mods.
    /// It must be called in the "Bootstrapper" context.
    /// </remarks>
    public static Configurator TryBindingModdableAudioClip(this Configurator configurator)
    {
        if (configurator.IsBound<ModAudioClipConverter>()) { return configurator; }

        configurator.Bind<ModAudioClipConverter>().AsSingleton().AsExported();
        configurator.Bind<IModFileConverter<AudioClip>>().ToExisting<ModAudioClipConverter>();

        configurator.Bind<ModSystemFileProvider<AudioClip>>().AsSingleton().AsExported();
        configurator.MultiBind<IAssetProvider>().ToExisting<ModSystemFileProvider<AudioClip>>();

        return configurator;
    }

    public static Configurator TryBindingAudioClipManagement(this Configurator configurator)
    {
        configurator.TryBind<AudioClipManagementService>()?.AsSingleton();
        return configurator;
    }

    /// <summary>
    /// Try to bind the SystemFileDialogService, which allows opening system file dialogs.
    /// </summary>
    /// <remarks>
    /// This method can be called multiple times in case it's called from multiple mods.
    /// </remarks>
    public static Configurator TryBindingSystemFileDialogService(this Configurator configurator)
        => ISystemFileDialogService.TryBinding(configurator);

    /// <summary>
    /// Try to bind the SpriteOperationsConfigurator so you can deserialize Specs with Sprite. Only call this in MainMenu context.
    /// </summary>
    public static Configurator TryBindingSpriteOperations(this Configurator configurator)
    {
        if (configurator.IsBound<SpriteResizer>()) { return configurator; }

        new SpriteOperationsConfigurator().Configure(configurator._containerDefinition);
        return configurator;
    }

}
