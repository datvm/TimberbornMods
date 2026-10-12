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
internal static class FrameProfileSampler
{
    private static MethodBase? _target;
    private static bool _patched;
    private static long _startTick;
    private static long _sampleTick;
    private static int _frames;
    private static int _lastSampleFrame;
    private static readonly List<string> Rows = new();

    public static void Patch(Harmony harmony)
    {
        if (_patched) return;
        var lifecycle = AccessTools.TypeByName("Timberborn.SingletonSystem.SingletonLifecycleService");
        _target = lifecycle is null ? null : AccessTools.Method(lifecycle, "UpdateSingletons");
        if (_target is null) return;
        try
        {
            harmony.Patch(_target, postfix: new HarmonyMethod(AccessTools.Method(typeof(FrameProfileSampler), nameof(AfterUpdate))));
            Rows.Clear();
            Rows.Add("Frame,ElapsedSeconds,FrameMsAvg,FPS,GC0,GC1,GC2,ManagedHeapBytes");
            _startTick = _sampleTick = Stopwatch.GetTimestamp();
            _frames = _lastSampleFrame = 0;
            _patched = true;
        }
        catch { _target = null; _patched = false; }
    }

    private static void AfterUpdate()
    {
        if (!_patched || !Runtime.IsBenchmarking) return;
        ++_frames;
        if (_frames - _lastSampleFrame < 10) return;
        var now = Stopwatch.GetTimestamp();
        var n = _frames - _lastSampleFrame;
        var seconds = (now - _sampleTick) / (double)Stopwatch.Frequency;
        if (seconds <= 0) return;
        Rows.Add(FormattableString.Invariant(
            $"{_frames},{(now - _startTick) / (double)Stopwatch.Frequency:F3},{1000 * seconds / n:F3},{n / seconds:F2},{GC.CollectionCount(0)},{GC.CollectionCount(1)},{GC.CollectionCount(2)},{GC.GetTotalMemory(false)}"));
        _sampleTick = now;
        _lastSampleFrame = _frames;
    }

    public static void StopAndWrite(Harmony? harmony, string modPath)
    {
        if (!_patched) return;
        _patched = false;
        if (harmony is not null && _target is not null)
        {
            try { harmony.Unpatch(_target, HarmonyPatchType.Postfix, harmony.Id); }
            catch { /* Never alter the caller's benchmark completion. */ }
        }
        _target = null;
        try
        {
            File.WriteAllLines(Path.Combine(modPath,
                $"optimizer-v11-frames-{DateTime.Now:yyyyMMdd-HHmmss}.csv"), Rows);
        }
        catch { /* CSV profiling failure must not break gameplay. */ }
        Rows.Clear();
    }
}
