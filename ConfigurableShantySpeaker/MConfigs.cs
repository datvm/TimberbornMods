global using ConfigurableShantySpeaker.Components;
global using ConfigurableShantySpeaker.UI;
global using ConfigurableShantySpeaker.Services;
global using Mods.ShantySpeaker.Scripts;

namespace ConfigurableShantySpeaker;

[Context("Bootstrapper")]
public class ModBootstrapperConfig : Configurator
{
    public override void Configure()
    {
        this.TryBindingModdableAudioClip();
    }
}

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindTemplateModule(h => h.AddDecorator<FinishableBuildingSoundPlayerSpec, ShantySpeakerConfigComponent>())
            .BindFragment<ShantySpeakerFragment>()
            .BindSingleton<ShantySoundService>()

            .TryBindingAudioClipManagement()
        ;
    }
}

public class MStarter : IModStarter
{

    public static string ModPath { get; private set; } = "";

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        ModPath = modEnvironment.ModPath;
        new Harmony(nameof(ConfigurableShantySpeaker)).PatchAll();
    }
}
