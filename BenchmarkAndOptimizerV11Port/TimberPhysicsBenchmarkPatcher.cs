using System.Collections;
using System.Reflection;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using HarmonyLib;
using Timberborn.ModManagerScene;

namespace BenchmarkAndOptimizerV11;

internal static class TimberPhysicsBenchmarkPatcher
{
    private const string SimulatorTypeName = "TimberPhysics.Core.PhysicsSimulator";
    private const string RegistryTypeName = "TimberPhysics.Core.PhysicalObjectRegistry";
    private const string PhysicsTypeName = "UnityEngine.Physics";

    private static bool _patched;
    private static MethodBase? _simulatorMethod;
    private static MethodBase? _stepAllMethod;
    private static MethodBase? _simulateMethod;
    private static Type? _simulatorType;
    private static string _origin = "MOD: Bober's Laws of Motion (eMka.TimberPhysics)";

    [ThreadStatic]
    private static int _simulatorDepth;

    public static void Patch(Harmony harmony)
    {
        if (_patched)
        {
            return;
        }

        _simulatorType = AccessTools.TypeByName(SimulatorTypeName);
        var registryType = AccessTools.TypeByName(RegistryTypeName);
        var physicsType = AccessTools.TypeByName(PhysicsTypeName);

        _simulatorMethod = _simulatorType is null
            ? null
            : AccessTools.Method(_simulatorType, "UpdateSingleton", Type.EmptyTypes);
        _stepAllMethod = registryType is null
            ? null
            : AccessTools.Method(registryType, "StepAll", new[] { typeof(float) });
        _simulateMethod = physicsType is null
            ? null
            : AccessTools.Method(physicsType, "Simulate", new[] { typeof(float) });

        if (_simulatorMethod is null || _stepAllMethod is null || _simulateMethod is null)
        {
            Runtime.Log(
                $"warning: TimberPhysics split benchmark unavailable; " +
                $"simulator={_simulatorMethod is not null}, stepAll={_stepAllMethod is not null}, " +
                $"simulate={_simulateMethod is not null}");
            _simulatorMethod = null;
            _stepAllMethod = null;
            _simulateMethod = null;
            _simulatorType = null;
            return;
        }

        _origin = Runtime.ResolveDiagnosticOrigin(_simulatorType);

        harmony.Patch(
            _simulatorMethod,
            prefix: new HarmonyMethod(AccessTools.Method(typeof(TimberPhysicsBenchmarkPatcher), nameof(SimulatorPrefix))) { priority = Priority.First },
            finalizer: new HarmonyMethod(AccessTools.Method(typeof(TimberPhysicsBenchmarkPatcher), nameof(SimulatorFinalizer))));

        harmony.Patch(
            _stepAllMethod,
            prefix: new HarmonyMethod(AccessTools.Method(typeof(TimberPhysicsBenchmarkPatcher), nameof(StepAllPrefix))),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(TimberPhysicsBenchmarkPatcher), nameof(StepAllPostfix))));

        harmony.Patch(
            _simulateMethod,
            prefix: new HarmonyMethod(AccessTools.Method(typeof(TimberPhysicsBenchmarkPatcher), nameof(SimulatePrefix))),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(TimberPhysicsBenchmarkPatcher), nameof(SimulatePostfix))));

        _patched = true;
        Runtime.Log("TimberPhysics split benchmark installed: StepAll + Physics.Simulate");
    }

    public static void Unpatch(Harmony harmony)
    {
        if (!_patched)
        {
            return;
        }

        foreach (var method in new[] { _simulatorMethod, _stepAllMethod, _simulateMethod })
        {
            if (method is not null)
            {
                harmony.Unpatch(method, HarmonyPatchType.All, harmony.Id);
            }
        }

        _patched = false;
        _simulatorDepth = 0;
        Runtime.Log("TimberPhysics split benchmark removed");
    }

    private static void SimulatorPrefix()
    {
        if (Runtime.IsBenchmarking)
        {
            _simulatorDepth++;
        }
    }

    private static Exception? SimulatorFinalizer(Exception? __exception)
    {
        if (_simulatorDepth > 0)
        {
            _simulatorDepth--;
        }

        return __exception;
    }

    private static void StepAllPrefix(out long __state)
    {
        __state = _simulatorDepth > 0 && Runtime.IsBenchmarking
            ? System.Diagnostics.Stopwatch.GetTimestamp()
            : 0;
    }

    private static void StepAllPostfix(long __state)
    {
        if (__state == 0)
        {
            return;
        }

        Runtime.RecordDiagnosticBenchmark(
            "TimberPhysics.Core.PhysicsSimulator::StepAll",
            _origin,
            System.Diagnostics.Stopwatch.GetTimestamp() - __state);
    }

    private static void SimulatePrefix(out long __state)
    {
        __state = _simulatorDepth > 0 && Runtime.IsBenchmarking
            ? System.Diagnostics.Stopwatch.GetTimestamp()
            : 0;
    }

    private static void SimulatePostfix(long __state)
    {
        if (__state == 0)
        {
            return;
        }

        Runtime.RecordDiagnosticBenchmark(
            "TimberPhysics.Core.PhysicsSimulator::Physics.Simulate",
            _origin,
            System.Diagnostics.Stopwatch.GetTimestamp() - __state);
    }
}
