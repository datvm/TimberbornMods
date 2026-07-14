namespace UnstableCoreChallenge.Services;

[MultiBind(typeof(IDevModule))]
public class UnstableCoreChallengeDevModule(
    UnstableCoreSpawner spawner,
    EntitySelectionService entitySelectionService
) : IDevModule
{

    public DevModuleDefinition GetDefinition()
    {
        return new DevModuleDefinition.Builder()
            .AddMethod(DevMethod.Create("Unstable Core: Try Spawning", spawner.TrySpawningCore))
            .AddMethod(DevMethod.Create("Unstable Core: Fill up payment", FillUpPayment))
            .Build();
    }

    void FillUpPayment()
    {
        var e = entitySelectionService.SelectedObject;
        if (!e) { return; }

        var stabilizer = e.GetComponent<UnstableCoreStabilizer>();
        if (!stabilizer) { return; }

        stabilizer.DebugPayAll();
    }

}
