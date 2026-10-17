using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using HarmonyLib;

namespace BenchmarkAndOptimizerV11;

/// <summary>
/// Optional, read-only frame/GC samples during the existing 120s CSV benchmark.
/// Records one row per ten Unity UpdateSingletons calls, and never skips game logic.
/// This does not turn on legacy scene-wide, mod-specific or invasive profilers.
/// </summary>
static class FrameProfileSampler
{
    static MethodBase? targetMethod;
    static bool isPatched;
    static long startTick;
    static long sampleTick;
    static int frames;
    static int lastSampleFrame;
    static readonly List<string> Rows = [];

    public static void Patch(Harmony harmony)
    {
        if (isPatched) { return; }
        var lifecycle = AccessTools.TypeByName("Timberborn.SingletonSystem.SingletonLifecycleService");
        targetMethod = lifecycle is null ? null : AccessTools.Method(lifecycle, "UpdateSingletons");
        if (targetMethod is null) { return; }
        try
        {
            harmony.Patch(targetMethod, postfix: new HarmonyMethod(AccessTools.Method(typeof(FrameProfileSampler), nameof(AfterUpdate))));
            Rows.Clear();
            Rows.Add("Frame,ElapsedSeconds,FrameMsAvg,FPS,GC0,GC1,GC2,ManagedHeapBytes");
            startTick = sampleTick = Stopwatch.GetTimestamp();
            frames = lastSampleFrame = 0;
            isPatched = true;
        }
        catch { targetMethod = null; isPatched = false; }
    }

    static void AfterUpdate()
    {
        if (!isPatched || !Runtime.IsBenchmarking) { return; }
        ++frames;
        if (frames - lastSampleFrame < 10) { return; }
        var now = Stopwatch.GetTimestamp();
        var n = frames - lastSampleFrame;
        var seconds = (now - sampleTick) / (double)Stopwatch.Frequency;
        if (seconds <= 0) { return; }
        Rows.Add(FormattableString.Invariant(
            $"{frames},{(now - startTick) / (double)Stopwatch.Frequency:F3},{1000 * seconds / n:F3},{n / seconds:F2},{GC.CollectionCount(0)},{GC.CollectionCount(1)},{GC.CollectionCount(2)},{GC.GetTotalMemory(false)}"));
        sampleTick = now;
        lastSampleFrame = frames;
    }

    public static void StopAndWrite(Harmony? harmony, string modPath)
    {
        if (!isPatched) { return; }
        isPatched = false;
        if (harmony is not null && targetMethod is not null)
        {
            try { harmony.Unpatch(targetMethod, HarmonyPatchType.Postfix, harmony.Id); }
            catch { /* Never alter the caller's benchmark completion. */ }
        }
        targetMethod = null;
        try
        {
            File.WriteAllLines(Path.Combine(modPath,
                $"optimizer-v11-frames-{DateTime.Now:yyyyMMdd-HHmmss}.csv"), Rows);
        }
        catch { /* CSV profiling failure must not break gameplay. */ }
        Rows.Clear();
    }
}
