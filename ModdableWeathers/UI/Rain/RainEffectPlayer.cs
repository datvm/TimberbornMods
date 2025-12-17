namespace ModdableWeathers.UI.Rain;

public class RainEffectPlayer(
    IEnumerable<IRainEffect> rainEffects
) : ILoadableSingleton
{

    const float RainParticlePerSquare = 200f / (32 * 32);
    const int MaxParticleRate = 1000;
    const float WindStrengthModifier = 5f;
    const float ParticleLifetime = 2.5f;


    public bool IsRaining { get; private set; }

    public readonly ImmutableArray<IRainEffect> RainEffects = [.. rainEffects];

    GameObject? rainObj;
    ParticleSystem ps = null!;
    Material material = null!;

    public void Load()
    {

    }

    public bool CanRain
    {
        get;
        set;
    }

}
