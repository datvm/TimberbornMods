namespace BenchmarkAndOptimizer.Services;

public class GameBenchmarkService(
    EventBus eb,
    IExplorerOpener opener
) : BaseBenchmarkService, ITickableSingleton
{
    public static readonly string OutputFolder = BenchmarkLogger.RootDirectory;

    public bool IsBenchmarking { get; private set; }
    public GameBenchmarkResult? Result { get; private set; }

    float endingTime;
    public float RemainingTime => IsBenchmarking ? endingTime - Time.unscaledTime : 0;

    public void StartBenchmarking(float duration)
    {
        if (IsBenchmarking)
        {
            throw new InvalidOperationException("Benchmarking is already in progress.");
        }

        IsBenchmarking = true;
        Result = new GameBenchmarkResult(Time.unscaledTime, duration);
        endingTime = Time.unscaledTime + duration;
    }

    public BenchmarkTracker? Track(object entity)
    {
        return Result?.Track(entity);
    }

    public void Tick()
    {
        if (!IsBenchmarking) { return; }

        if (endingTime <= Time.unscaledTime)
        {
            OnBenchmarkEnd();
        }
    }

    public void EndBenchmarking() => endingTime = 0;

    void SaveResultToFile()
    {
        Directory.CreateDirectory(OutputFolder);
        var fileName = $"result_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var filePath = Path.Combine(OutputFolder, fileName);

        var content = "Type,Total,Count,Average,Min,Max" + Environment.NewLine;
        var typeLen = Result!.Summary!.Value.Max(q => q.Type.Name.Length) + 2;
        content += string.Join(Environment.NewLine, Result.Summary.Value.Select(q =>
            $"{q.Type.Name.PadRight(typeLen)},{q.Total},{q.SampleCount},{q.Average},{q.Min},{q.Max}"));

        ModUtils.Log(() => $"Saving benchmark result to {filePath}");
        ModUtils.Log(() => content);

        File.WriteAllText(filePath, content);

        opener.OpenDirectory(OutputFolder);
    }

    void OnBenchmarkEnd()
    {
        ModUtils.Log(() => $"Benchmarking completed.");

        IsBenchmarking = false;
        Result!.EndingTime = Time.unscaledTime;
        Result.Summary = SummarizeResults(Result.Samples);

        SaveResultToFile();

        eb.Post(new OnBenchmarkEndEvent(Result));
    }

    ImmutableArray<BenchmarkSummaryEntry> SummarizeResults(List<BenchmarkSample> samples)
    {
        var summary = new List<BenchmarkSummaryEntry>();

        var grps = samples.GroupBy(q => q.Type);

        foreach (var grp in grps)
        {
            float min = float.MaxValue, max = float.MinValue, total = 0;
            int count = 0;

            foreach (var item in grp)
            {
                var time = item.Time;
                if (time < min) { min = time; }
                if (time > max) { max = time; }
                total += time;
                count++;
            }

            float average = total / count;
            summary.Add(new BenchmarkSummaryEntry(grp.Key, min, max, count, average, total));
        }

        return [.. summary.OrderByDescending(q => q.Total)];
    }

}

public readonly record struct OnBenchmarkEndEvent(GameBenchmarkResult Result);

public record GameBenchmarkResult(float StartingTime, float Duration)
{
    public List<BenchmarkSample> Samples { get; } = [];

    public ImmutableArray<BenchmarkSummaryEntry>? Summary { get; set; }
    public float? EndingTime { get; set; }

    public BenchmarkTracker Track(object entity) => new(entity.GetType(), this);

}

public readonly record struct BenchmarkTracker(Type Type, GameBenchmarkResult Result)
{
    readonly System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

    public BenchmarkSample End()
    {
        sw.Stop();
        if (Result is null) { return default; }

        var sample = new BenchmarkSample(Type, (float)sw.Elapsed.TotalMilliseconds);
        Result.Samples.Add(sample);
        return sample;
    }

}

public readonly record struct BenchmarkSample(Type Type, float Time);
public readonly record struct BenchmarkSummaryEntry(Type Type, float Min, float Max, int SampleCount, float Average, float Total);