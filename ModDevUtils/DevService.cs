using Timberborn.AssetSystem;
using UiBuilder.Common;

namespace ModDevUtils;

[Context("MainMenu")]
public class DevServiceConfig : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<DevService>().AsSingleton();
    }
}

public class DevService(DialogBoxShower diag, IAssetLoader assets, VisualElementLoader ve, ILoc loc) : ILoadableSingleton
{

    public async void Load()
    {
        await Task.CompletedTask;
    }


}
