namespace ModdableWeather.Services;

/// <summary>
/// Registry for all moddable weather types, providing lookup and access to weather definitions and their specifications.
/// </summary>
public class ModdableWeatherRegistry(
    IEnumerable<IModdedTemperateWeather> temperateWeathers,
    IEnumerable<IModdedHazardousWeather> hazardousWeathers,
    NoneHazardousWeather noneHazardousWeather,
    GameTemperateWeather gameTemperateWeather,
    GameDroughtWeather gameDroughtWeather,
    GameBadtideWeather gameBadtideWeather
)
{
    /// <summary>
    /// Gets the built-in temperate weather instance.
    /// </summary>
    public GameTemperateWeather GameTemperateWeather { get; } = gameTemperateWeather;

    /// <summary>
    /// Gets the built-in drought weather instance.
    /// </summary>
    public GameDroughtWeather GameDroughtWeather { get; } = gameDroughtWeather;

    /// <summary>
    /// Gets the built-in badtide weather instance.
    /// </summary>
    public GameBadtideWeather GameBadtideWeather { get; } = gameBadtideWeather;

    /// <summary>
    /// Gets the special non-hazardous weather instance.
    /// </summary>
    public NoneHazardousWeather NoneHazardousWeather { get; } = noneHazardousWeather;

    /// <summary>
    /// Gets all registered temperate weather types.
    /// </summary>
    public ImmutableHashSet<IModdedTemperateWeather> TemperateWeathers { get; } = [.. temperateWeathers];

    /// <summary>
    /// Gets all registered hazardous weather types.
    /// </summary>
    public ImmutableHashSet<IModdedHazardousWeather> HazardousWeathers { get; } = [.. hazardousWeathers];

    /// <summary>
    /// Gets all registered weather types (both temperate and hazardous).
    /// </summary>
    public ImmutableHashSet<IModdedWeather> AllWeathers { get; } = [
        .. temperateWeathers,
        .. hazardousWeathers
    ];

    /// <summary>
    /// Gets a dictionary mapping weather IDs to their corresponding weather objects.
    /// </summary>
    public FrozenDictionary<string, IModdedWeather> WeatherByIds { get; } = 
        temperateWeathers
        .Concat<IModdedWeather>(hazardousWeathers)
        .ToFrozenDictionary(q => q.Id);

    /// <summary>
    /// Gets a temperate weather instance by its ID.
    /// </summary>
    /// <param name="id">The weather ID.</param>
    /// <returns>The temperate weather instance.</returns>
    public IModdedTemperateWeather GetTemperateWeather(string id) => (IModdedTemperateWeather)WeatherByIds[id];

    /// <summary>
    /// Gets a hazardous weather instance by its ID.
    /// </summary>
    /// <param name="id">The weather ID.</param>
    /// <returns>The hazardous weather instance.</returns>
    public IModdedHazardousWeather GetHazardousWeather(string id) => (IModdedHazardousWeather)WeatherByIds[id];

    /// <summary>
    /// Checks if a weather type with the specified ID exists in the registry.
    /// </summary>
    public bool HasWeather(string id) => WeatherByIds.ContainsKey(id);

}
