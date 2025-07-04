global using BeaverJukebox.Services;
global using BeaverJukebox.UI;

namespace BeaverJukebox;

public class CommonConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<MSettings>()
            .BindTransient<JukeboxModSettingPanel>()
            .MultiBindSingleton<IModSettingElementFactory, JukeboxModSettingFactory>()

            .BindSingleton<DemoSoundPlayer>()
            
            .MassRebind(h =>
                h.Replace<AudioClipService, ModdableAudioClipService>())

            .TryBindingSystemFileDialogService()
        ;
    }
}

[Context("Bootstrapper")]
public class ModBootstrapperConfig : Configurator
{
    public override void Configure()
    {
        Bind<AudioMuteService>().AsSingleton().AsExported();
    }
}

[Context("MainMenu")]
public class ModMenuConfig : CommonConfig
{

    public override void Configure()
    {
        base.Configure();
    }

}

[Context("Game")]
public class ModGameConfig : CommonConfig
{

    public override void Configure()
    {
        base.Configure();
    }

}
