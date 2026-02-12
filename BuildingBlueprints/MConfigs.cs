global using BuildingBlueprints.UI;
global using BuildingBlueprints.Tools;
global using BuildingBlueprints.Services;

namespace BuildingBlueprints;

[Context(nameof(BindAttributeContext.Game))]
public class MGameConfig : GameAttributeConfigurator
{

    public override void Configure()
    {
        base.Configure();

        this
            .TryBindingSystemFileDialogService()

            .MultiBindCustomTool<BuildingBlueprintsButtons>()
        ;
    }

}