global using MerryChristmas2025.Services;

namespace MerryChristmas2025;

[Context("Bootstrapper")]
public class MBootstrapperConfig : Configurator
{
    public override void Configure()
    {
        this
            .TryBindingModdableAudioClip();
        ;
    }
}

[Context("Game")]
public class MGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<DynamiteTrackingService>()
        ;
    }
}
