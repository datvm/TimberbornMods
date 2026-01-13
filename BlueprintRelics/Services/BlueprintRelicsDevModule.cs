namespace BlueprintRelics.Services;

public class BlueprintRelicsDevModule(BlueprintRelicsSpawner spawner) : IDevModule
{
    public DevModuleDefinition GetDefinition() => new DevModuleDefinition.Builder()
        .AddMethod(DevMethod.Create("Blueprint Relics: Try spawning", TrySpawn))
        .AddMethod(DevMethod.Create("Blueprint Relics: Guaranteed spawn", GuaranteedSpawn))
        .Build();

    void TrySpawn() => spawner.TryToSpawn();
    void GuaranteedSpawn() => spawner.Spawn();

}
