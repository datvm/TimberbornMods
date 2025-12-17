namespace ModdableWeathers.WeatherModifiers;

public delegate void WeatherModifierChangedEventHandler(IModdableWeatherModifier modifier, bool active, bool onLoad);