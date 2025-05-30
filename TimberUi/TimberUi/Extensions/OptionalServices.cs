namespace Bindito.Core;

public static partial class UiBuilderExtensions
{

    /// <summary>
    /// Bind services needed for camera shake (can be called multiple times in case it's called from multiple mods).
    /// </summary>
    public static Configurator TryAddingCameraShake(this Configurator configurator, bool isMenuContext)
    {
        if (!isMenuContext)
        {
            configurator.TryBind<CameraShakeService>()?.AsSingleton();
        }
        configurator.TryBind<CameraShakeSettingService>()?.AsSingleton();

        return configurator;
    }


}
