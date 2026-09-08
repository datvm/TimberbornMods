namespace BeavlineLogistics.Services;

public class BeavlineDevModule(
    EntitySelectionService entitySelectionService,
    DialogService diag
) : IDevModule
{

    public DevModuleDefinition GetDefinition()
    {
        return new DevModuleDefinition.Builder()
            .AddMethod(DevMethod.Create("Beavline: Send output now", SendStuffNow))
            .Build();
    }

    void SendStuffNow()
    {
        var entity = entitySelectionService.SelectedObject;
        var comp = entity ? entity.GetComponentFast<BeavlineOutputComponent>() : null;
        if (!comp || !comp.beavline.HasOutput || comp.beavline.DisableOutput)
        {
            diag.Alert("Pick a Beavline with non-disabled output.");
            return;
        }

        comp.debugging = true;
        comp.FindAndMoveStuffOut();
        comp.debugging = false;
    }

}
