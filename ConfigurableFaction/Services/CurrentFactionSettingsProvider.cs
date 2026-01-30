namespace ConfigurableFaction.Services;

[BindSingleton]
public class CurrentFactionSettingsProvider(
    ISingletonLoader singletonLoader,
    ISceneLoader sceneLoader,
    UserSettingsService userSettingsService
) : ILoadableSingleton
{

    public string CurrentFactionId { get; private set; } = "";
    public FactionUserSetting CurrentSettings { get; private set; } = null!;

    public void Load()
    {
        if (singletonLoader.TryGetSingleton(FactionService.FactionServiceKey, out var s))
        {
            CurrentFactionId = s.Get(FactionService.IdKey);
        }
        else
        {
            var p = sceneLoader.GetSceneParameters<GameSceneParameters>();
            CurrentFactionId = p.NewGameConfiguration.FactionId;
        }

        CurrentSettings = userSettingsService.GetOrAddFaction(CurrentFactionId);
    }

}
