namespace BenchmarkAndOptimizer;

internal static class ModUtils
{

    public static void Log(Func<string> message)
    {
        Debug.Log($"{DateTime.Now:HH:mm:ss.fff}: {nameof(BenchmarkAndOptimizer)}: {message()}");
    }

    public static string ToLogString(this TimeSpan duration) => $"{duration.TotalMilliseconds:#,0}ms";

}
