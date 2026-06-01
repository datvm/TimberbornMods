namespace BeaverChronicles.Services;

[BindSingleton]
public class EvaluationCacheService
{

    int lastFrame = -1;
    readonly Dictionary<string, object> caches = [];

    public T GetOrEvaluate<T>(string key, Func<T> evaluate, bool force = false)
    {
        if (Time.frameCount != lastFrame)
        {
            caches.Clear();
        }
        else if(!force && caches.TryGetValue(key, out var cached))
        {
            return (T)cached;
        }

        var value = evaluate();
        caches[key] = value!;
        lastFrame = Time.frameCount;
        return value;
    }

}
