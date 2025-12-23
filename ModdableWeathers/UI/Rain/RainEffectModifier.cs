namespace ModdableWeathers.UI.Rain;

public record RainEffectModifier(string Id, int Priority, Color? RainColor) : IWeatherEntityModifierEntry;