namespace BuildingHP.UI;

public class RenovationDialogController(IContainer container)
{
    RenovationDialog? curr;

    public bool IsOpen => curr is not null;

    public async Task OpenDialogAsync(BuildingRenovationComponent comp)
    {
        curr = container.GetInstance<RenovationDialog>();
        curr.SetDialogPercentSize(.6f, .6f);
        await curr.ShowAsync(comp);

        curr = null;
    }

    public void CloseDialog()
    {
        curr?.Close();
    }

}
