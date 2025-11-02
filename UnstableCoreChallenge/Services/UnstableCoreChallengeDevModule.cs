namespace UnstableCoreChallenge.Services;

public class UnstableCoreChallengeDevModule(
    UnstableCoreSpawner spawner
) : IDevModule
{

    public DevModuleDefinition GetDefinition()
    {
        return new DevModuleDefinition.Builder()
            .AddMethod(DevMethod.Create("Spawn Unstable Core", spawner.SpawnCore))
            .Build();
    }

}
