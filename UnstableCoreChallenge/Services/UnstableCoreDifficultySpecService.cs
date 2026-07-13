namespace UnstableCoreChallenge.Services;

[BindSingleton(Contexts = BindAttributeContext.MainMenu | BindAttributeContext.Game)]
public class UnstableCoreDifficultySpecService(ISpecService specs) : ILoadableSingleton
{

    public ImmutableArray<UnstableCoreChallengeDifficultySpec> Difficulties { get; private set; }

    FrozenDictionary<string, UnstableCoreChallengeDifficultySpec> diffByNames = null!;
    public ImmutableArray<string> DifficultyNames => diffByNames.Keys;

    public UnstableCoreChallengeDifficultySpec GetDifficulty(string name) => diffByNames[name];

    public void Load()
    {
        Difficulties = [.. specs
            .GetSpecs<UnstableCoreChallengeDifficultySpec>()
            .OrderBy(d => d.Order)];
        diffByNames = Difficulties.ToFrozenDictionary(d => d.Name.Value);
    }

}
