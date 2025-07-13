namespace BenchmarkAndOptimizer.Services;

public class GameBenchmarkService : BaseBenchmarkService
{
    public GameBenchmarkResult? Result { get; private set; }

    public void StartBenchmark(float durationSeconds)
    {
        Result = new(Time.time, durationSeconds);
        StartTime = DateTime.Now;
        LastTime = DateTime.Now;
    }

    public void EndBenchmark()
    {
        if (Result == null)
        {
            throw new InvalidOperationException("Benchmark has not been started.");
        }

        Result.EndingTime = Time.time;
        CalculateResults();
    }

    public bool IsExpired()
    {
        if (Result == null)
        {
            return false;
        }

        return Time.time >= Result.StartingTime + Result.DurationSeconds;
    }

    public void AddTickSample(Type componentType, float duration)
    {
        if (Result == null)
        {
            throw new InvalidOperationException("Benchmark has not been started.");
        }

        if (IsExpired())
        {
            return; // Don't collect samples after benchmark expires
        }

        Result.TickSamples.Add(new BenchmarkSample(componentType, duration));
    }

    public void AddUpdateSample(Type componentType, float duration)
    {
        if (Result == null)
        {
            throw new InvalidOperationException("Benchmark has not been started.");
        }

        if (IsExpired())
        {
            return; // Don't collect samples after benchmark expires
        }

        Result.UpdateSamples.Add(new BenchmarkSample(componentType, duration));
    }

    public void BenchmarkTickables<T>(T[] tickables, Action<T> tickAction) where T : notnull
    {
        if (Result == null)
        {
            throw new InvalidOperationException("Benchmark has not been started.");
        }

        if (IsExpired())
        {
            return; // Don't benchmark if expired
        }

        for (int i = 0; i < tickables.Length; i++)
        {
            if (IsExpired()) break; // Check expiration during loop

            var startTime = Time.time;
            tickAction(tickables[i]);
            var endTime = Time.time;
            
            AddTickSample(typeof(T), endTime - startTime);
        }
    }

    public void BenchmarkUpdatables<T>(T[] updatables, Action<T> updateAction) where T : notnull
    {
        if (Result == null)
        {
            throw new InvalidOperationException("Benchmark has not been started.");
        }

        if (IsExpired())
        {
            return; // Don't benchmark if expired
        }

        for (int i = 0; i < updatables.Length; i++)
        {
            if (IsExpired()) break; // Check expiration during loop

            var startTime = Time.time;
            updateAction(updatables[i]);
            var endTime = Time.time;
            
            AddUpdateSample(typeof(T), endTime - startTime);
        }
    }

    public void BenchmarkTickables<T>(IReadOnlyList<T> tickables, Action<T> tickAction) where T : notnull
    {
        if (Result == null)
        {
            throw new InvalidOperationException("Benchmark has not been started.");
        }

        if (IsExpired())
        {
            return; // Don't benchmark if expired
        }

        for (int i = 0; i < tickables.Count; i++)
        {
            if (IsExpired()) break; // Check expiration during loop

            var startTime = Time.time;
            tickAction(tickables[i]);
            var endTime = Time.time;
            
            AddTickSample(typeof(T), endTime - startTime);
        }
    }

    public void BenchmarkUpdatables<T>(IReadOnlyList<T> updatables, Action<T> updateAction) where T : notnull
    {
        if (Result == null)
        {
            throw new InvalidOperationException("Benchmark has not been started.");
        }

        if (IsExpired())
        {
            return; // Don't benchmark if expired
        }

        for (int i = 0; i < updatables.Count; i++)
        {
            if (IsExpired()) break; // Check expiration during loop

            var startTime = Time.time;
            updateAction(updatables[i]);
            var endTime = Time.time;
            
            AddUpdateSample(typeof(T), endTime - startTime);
        }
    }

    public void BenchmarkTickComponent<T>(T component, Action<T> tickAction) where T : notnull
    {
        if (Result == null)
        {
            throw new InvalidOperationException("Benchmark has not been started.");
        }

        if (IsExpired())
        {
            return; // Don't benchmark if expired
        }

        var startTime = Time.time;
        tickAction(component);
        var endTime = Time.time;

        AddTickSample(typeof(T), endTime - startTime);
    }

    public void BenchmarkUpdateComponent<T>(T component, Action<T> updateAction) where T : notnull
    {
        if (Result == null)
        {
            throw new InvalidOperationException("Benchmark has not been started.");
        }

        if (IsExpired())
        {
            return; // Don't benchmark if expired
        }

        var startTime = Time.time;
        updateAction(component);
        var endTime = Time.time;

        AddUpdateSample(typeof(T), endTime - startTime);
    }

    private void CalculateResults()
    {
        if (Result == null)
        {
            return;
        }

        Result.TickResults = CalculateResultsForSamples(Result.TickSamples);
        Result.UpdateResults = CalculateResultsForSamples(Result.UpdateSamples);
    }

    private Dictionary<Type, BenchmarkResult> CalculateResultsForSamples(List<BenchmarkSample> samples)
    {
        if (samples.Count == 0)
        {
            return new Dictionary<Type, BenchmarkResult>();
        }

        var groupedSamples = samples
            .GroupBy(sample => sample.Type)
            .ToArray();

        var results = new Dictionary<Type, BenchmarkResult>();

        foreach (var group in groupedSamples)
        {
            var times = group.Select(s => s.Time * 1000f).ToArray(); // Convert to milliseconds
            var min = (int)times.Min();
            var max = (int)times.Max();
            var average = (int)times.Average();
            var total = (int)times.Sum();
            var sampleCount = times.Length;

            results[group.Key] = new BenchmarkResult(min, max, sampleCount, average, total);
        }

        return results;
    }
}

public record GameBenchmarkResult(float StartingTime, float DurationSeconds)
{
    public List<BenchmarkSample> TickSamples { get; } = [];
    public List<BenchmarkSample> UpdateSamples { get; } = [];

    public Dictionary<Type, BenchmarkResult>? TickResults { get; set; }
    public Dictionary<Type, BenchmarkResult>? UpdateResults { get; set; }
    
    public float? EndingTime { get; set; }
}

public readonly record struct BenchmarkSample(Type Type, float Time);
public readonly record struct BenchmarkResult(int Min, int Max, int SampleCount, int Average, int Total);