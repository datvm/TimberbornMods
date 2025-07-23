namespace BenchmarkAndOptimizer.Services;

public class GameOptimizerService(
    OptimizerSettings controller,
    GameBenchmarkService benchmark
) : ILoadableSingleton, IUnloadableSingleton
{

    public static GameOptimizerService? Instance { get; private set; }
    public bool Benchmarking => benchmark.IsBenchmarking;

    ConditionalWeakTable<object, GameOptimizerDelayer> delayers = [];

    public void Load()
    {
        Instance = this;
    }

    public void Unload()
    {
        Instance = null;
    }

    public bool RunList<T>(IList<T> collection, Action<T> action) where T : notnull => RunList(collection, action, item => item);

    public bool RunList<T, TActual>(IList<T> collection, Action<T> action, Func<T, TActual> trackingType) where TActual : notnull
    {
        var bm = Benchmarking;
        for (int i = 0; i < collection.Count; i++)
        {
            var item = collection[i];
            var actualItem = trackingType(item);

            if (!delayers.TryGetValue(actualItem, out var delayer))
            {
                var counter = GetDelayerCounter(actualItem);
                if (counter is not null)
                {
                    delayer = new();
                    delayers.AddOrUpdate(actualItem, delayer);
                }
            }

            if (delayer is not null && --delayer.Counter > 0)
            {
                continue;
            }

            BenchmarkTracker? tracker = bm ? benchmark.Track(actualItem) : null;
            action(item);
            tracker?.End();

            if (delayer is not null)
            {
                var value = GetDelayerCounter(actualItem);

                if (value is null)
                {
                    delayers.Remove(actualItem);
                }
                else
                {
                    delayer.Counter = value.Value;
                }
            }
        }

        return false;
    }

    int? GetDelayerCounter(object obj)
    {
        var value = controller.GetValue(obj.GetType());
        return value?.Enabled == true && value.Value > 0 ? value.Value : null;
    }

    public (bool ShouldRun, BenchmarkTracker? Benchmarker) RunComponentUpdate(BaseComponent component)
    {
        if (Benchmarking)
        {
            return (true, benchmark.Track(component));
        }
        else
        {
            return (true, null);
        }
    }

}

public class GameOptimizerDelayer
{
    public int Counter { get; set; }
}