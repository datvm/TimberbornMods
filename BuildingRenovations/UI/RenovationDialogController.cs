namespace BuildingRenovations.UI;

[Bind]
public class RenovationDialogController(IContainer container)
{
    RenovationDialog? curr;

    public bool IsOpen => curr is not null;

    public async Task OpenDialogAsync(BuildingRenovationComponent building)
    {
        curr = container.GetInstance<RenovationDialog>().Init();
        curr.SetDialogPercentSize(.6f, .6f);
        await curr.ShowAsync(building);
        curr = null;
    }

    public void CloseDialog() => curr?.Close();
}
