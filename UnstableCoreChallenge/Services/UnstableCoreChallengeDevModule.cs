namespace UnstableCoreChallenge.Services;

[MultiBind(typeof(IDevModule))]
public class UnstableCoreChallengeDevModule(
    UnstableCoreSpawner spawner
) : IDevModule
{

    public DevModuleDefinition GetDefinition()
    {
        return new DevModuleDefinition.Builder()
            .AddMethod(DevMethod.Create("Unstable Core: Try Spawning", spawner.TrySpawningCore))
            .Build();
    }

}
