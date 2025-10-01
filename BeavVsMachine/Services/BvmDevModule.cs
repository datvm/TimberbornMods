namespace BeavVsMachine.Services;

public class BvmDevModule(
    DialogService diag,
    EntitySelectionService entitySelectionService
) : IDevModule
{

    public DevModuleDefinition GetDefinition() =>
        new DevModuleDefinition.Builder()
            .AddMethod(DevMethod.Create("Set EXP", SetExp))
            .Build();

    async void SetExp()
    {
        var selecting = entitySelectionService.SelectedObject;
        var exp = selecting ? selecting.GetComponentFast<BeaverExpComponent>() : null;

        if (!exp)
        {
            diag.Alert("Select a beaver for this");
            return;
        }

        var input = await diag.PromptAsync("Enter EXP:", exp.Experience.ToString("0"));
        if (float.TryParse(input, out var newExp))
        {
            exp.SetExperience(newExp);
        }
    }

}
