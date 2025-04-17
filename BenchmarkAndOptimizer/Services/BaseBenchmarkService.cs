namespace BenchmarkAndOptimizer.Services;
public class BaseBenchmarkService
{

    public DateTime StartTime { get; protected set; }
    public DateTime LastTime { get; protected set; }

    public Dictionary<string, DateTime> TimeMarks { get; } = [];

    public static readonly TimeSpan LongTime = TimeSpan.FromSeconds(1);
    public static readonly TimeSpan ShortTimeCap = TimeSpan.FromMilliseconds(5);
    public void BenchmarkList<T>(IReadOnlyList<T> list, string markName, string itemMarkName, Action<T> action) where T : notnull
    {
        Dictionary<string, TimeSpan> timeDuration = [];

        MarkTime(markName);
        var count = list.Count;

        ModUtils.Log(() => $"Benchmarking {markName} with {count} items...");

        for (int i = 0; i < count; i++)
        {
            MarkTime(itemMarkName);

            var item = list[i];
            var name = item.GetType().FullName;

            action(item);

            var duration = GetTotalFor(itemMarkName);
            if (duration > LongTime)
            {
                ModUtils.Log(() => $"{name} took {duration.ToLogString()}");
            }

            timeDuration[name] = timeDuration.GetValueOrDefault(name) + duration;
        }

        ModUtils.Log(() => $"Benchmarking {markName} took {GetTotalFor(markName).ToLogString()}");

        var timeList = timeDuration
            .Where(q => q.Value > ShortTimeCap)
            .OrderByDescending(q => q.Value)
            .ToArray();

        if (timeList.Length > 0)
        {
            var maxLength = timeList.Max(q => q.Key.Length);

            ModUtils.Log(() => Environment.NewLine + string.Join(Environment.NewLine, timeList
                .Select(q => $"  {q.Key.PadLeft(maxLength)}: {q.Value.ToLogString(),10}")));
        }

        if (timeList.Length < timeDuration.Count)
        {
            ModUtils.Log(() => $"  ... and {timeDuration.Count - timeList.Length} more items each took less than {ShortTimeCap.ToLogString()}");
        }
    }

    public void CountEntities<T>(IReadOnlyList<T> list, Func<T, string> nameFunc) where T : notnull
    {
        Dictionary<string, int> counters = [];

        foreach (var item in list)
        {
            var name = nameFunc(item);
            counters[name] = counters.GetValueOrDefault(name) + 1;
        }

        var maxLength = counters.Max(q => q.Key.Length);
        ModUtils.Log(() => Environment.NewLine + string.Join(Environment.NewLine, counters
            .OrderByDescending(q => q.Value)
            .Select(q => $"  {q.Key.PadLeft(maxLength)}: {q.Value,10}")));
    }

    public TimeSpan MarkTime()
    {
        var result = DateTime.Now - LastTime;
        LastTime = DateTime.Now;

        return result;
    }

    public TimeSpan MarkTime(string name)
    {
        TimeMarks[name] = DateTime.Now;
        return MarkTime();
    }

    public TimeSpan GetTotalFor(string name)
    {
        return DateTime.Now - TimeMarks[name];
    }

}
