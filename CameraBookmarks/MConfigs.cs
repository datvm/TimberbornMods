global using CameraBookmarks.Services;

namespace CameraBookmarks;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<CameraBookmarksService>().AsSingleton();
    }
}
