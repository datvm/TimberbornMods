namespace BeaverChronicles.Services.Helpers;

[BindSingleton]
public class GameStatHelper(GameStatService service, EvaluationCacheService caches)
{

    public object? GetStat(string id) => caches.GetOrEvaluate(
        GetCacheKey(id),
        () => service.GetStat(id));

    public T? GetStat<T>(string id) => caches.GetOrEvaluate(
        GetCacheKey(id),
        () => service.GetStat<T>(id));

    public int GetIntStat(string id) => GetStat<int?>(id) ?? 0;
    public float GetFloatStat(string id) => GetStat<float?>(id) ?? 0f;

    public int GameDayNumber => GetIntStat(GameStats.TimeDayNumber);
    public float GameDayAndHours => GetFloatStat(GameStats.TimePartialDay);
    public int GameCycleNumber => GetIntStat(GameStats.TimeCycle);
    public int GameCycleDayNumber => GetIntStat(GameStats.TimeCycleDay);

    static string GetCacheKey(string id) => "GameStat." + id;
}
