namespace ModdableWeathers.UI.Rain;

public interface IRainEffect
{

    Color RainColor { get; }
    event RainEffectChangedEventHandler OnRainEffectChanged;

}

public delegate void RainEffectChangedEventHandler(bool active);