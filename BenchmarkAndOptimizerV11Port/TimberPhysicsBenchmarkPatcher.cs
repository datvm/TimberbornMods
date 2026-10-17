using System.Collections;
using System.Reflection;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using HarmonyLib;
using Timberborn.ModManagerScene;

namespace BenchmarkAndOptimizerV11;

static class TimberPhysicsBenchmarkPatcher
{
    const string SimulatorTypeName = "TimberPhysics.Core.PhysicsSimulator";
    const string RegistryTypeName = "TimberPhysics.Core.PhysicalObjectRegistry";
    const string PhysicsTypeName = "UnityEngine.Physics";

    static bool isPatched;
    static MethodBase? simulatorMethod;
    static MethodBase? stepAllMethod;
    static MethodBase? simulateMethod;
    static Type? simulatorType;
    static string originLabel = "MOD: Bober's Laws of Motion (eMka.TimberPhysics)";

    [ThreadStatic]
    static int simulatorDepth;

    public static void Patch(Harmony harmony)
    {
        if (isPatched)
        {
            return;
        }

        simulatorType = AccessTools.TypeByName(SimulatorTypeName);
        var registryType = AccessTools.TypeByName(RegistryTypeName);
        var physicsType = AccessTools.TypeByName(PhysicsTypeName);

        simulatorMethod = simulatorType is null
            ? null
            : AccessTools.Method(simulatorType, "UpdateSingleton", Type.EmptyTypes);
        stepAllMethod = registryType is null
            ? null
            : AccessTools.Method(registryType, "StepAll", new[] { typeof(float) });
        simulateMethod = physicsType is null
            ? null
            : AccessTools.Method(physicsType, "Simulate", new[] { typeof(float) });

        if (simulatorMethod is null || stepAllMethod is null || simulateMethod is null)
        {
            Runtime.Log(
                $"warning: TimberPhysics split benchmark unavailable; " +
                $"simulator={simulatorMethod is not null}, stepAll={stepAllMethod is not null}, " +
                $"simulate={simulateMethod is not null}");
            simulatorMethod = null;
            stepAllMethod = null;
            simulateMethod = null;
            simulatorType = null;
            return;
        }

        originLabel = Runtime.ResolveDiagnosticOrigin(simulatorType);

        harmony.Patch(
            simulatorMethod,
            prefix: new HarmonyMethod(AccessTools.Method(typeof(TimberPhysicsBenchmarkPatcher), nameof(SimulatorPrefix))) { priority = Priority.First },
            finalizer: new HarmonyMethod(AccessTools.Method(typeof(TimberPhysicsBenchmarkPatcher), nameof(SimulatorFinalizer))));

        harmony.Patch(
            stepAllMethod,
            prefix: new HarmonyMethod(AccessTools.Method(typeof(TimberPhysicsBenchmarkPatcher), nameof(StepAllPrefix))),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(TimberPhysicsBenchmarkPatcher), nameof(StepAllPostfix))));

        harmony.Patch(
            simulateMethod,
            prefix: new HarmonyMethod(AccessTools.Method(typeof(TimberPhysicsBenchmarkPatcher), nameof(SimulatePrefix))),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(TimberPhysicsBenchmarkPatcher), nameof(SimulatePostfix))));

        isPatched = true;
        Runtime.Log("TimberPhysics split benchmark installed: StepAll + Physics.Simulate");
    }

    public static void Unpatch(Harmony harmony)
    {
        if (!isPatched)
        {
            return;
        }

        foreach (var method in new[] { simulatorMethod, stepAllMethod, simulateMethod })
        {
            if (method is not null)
            {
                harmony.Unpatch(method, HarmonyPatchType.All, harmony.Id);
            }
        }

        isPatched = false;
        simulatorDepth = 0;
        Runtime.Log("TimberPhysics split benchmark removed");
    }

    static void SimulatorPrefix()
    {
        if (Runtime.IsBenchmarking)
        {
            simulatorDepth++;
        }
    }

    static Exception? SimulatorFinalizer(Exception? __exception)
    {
        if (simulatorDepth > 0)
        {
            simulatorDepth--;
        }

        return __exception;
    }

    static void StepAllPrefix(out long __state)
    {
        __state = simulatorDepth > 0 && Runtime.IsBenchmarking
            ? System.Diagnostics.Stopwatch.GetTimestamp()
            : 0;
    }

    static void StepAllPostfix(long __state)
    {
        if (__state == 0)
        {
            return;
        }

        Runtime.RecordDiagnosticBenchmark(
            "TimberPhysics.Core.PhysicsSimulator::StepAll",
            originLabel,
            System.Diagnostics.Stopwatch.GetTimestamp() - __state);
    }

    static void SimulatePrefix(out long __state)
    {
        __state = simulatorDepth > 0 && Runtime.IsBenchmarking
            ? System.Diagnostics.Stopwatch.GetTimestamp()
            : 0;
    }

    static void SimulatePostfix(long __state)
    {
        if (__state == 0)
        {
            return;
        }

        Runtime.RecordDiagnosticBenchmark(
            "TimberPhysics.Core.PhysicsSimulator::Physics.Simulate",
            originLabel,
            System.Diagnostics.Stopwatch.GetTimestamp() - __state);
    }
}
