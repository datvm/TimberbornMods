global using System.Collections.Frozen;
global using System.Diagnostics.CodeAnalysis;
global using Timberborn.BlueprintSystem;

namespace RealLights.Managements;

public class RealLightsManager(ISpecService specs, EventBus eb) : ILoadableSingleton
{
    FrozenDictionary<string, RealLightsSpec> realLightSpecs = null!;
    readonly HashSet<RealLightsComponent> instances = [];

    public void Load()
    {
        var lights = specs.GetSpecs<RealLightsSpec>();
        Dictionary<string, RealLightsSpec> dict = [];

        foreach (var spec in lights)
        {
            foreach (var name in spec.PrefabNames)
            {
                if (dict.ContainsKey(name))
                {
                    Debug.LogWarning($"{nameof(RealLightsSpec)} for prefab {name} already exists, overwriting...");
                }

                if (spec.Lights.Length == 0)
                {
                    throw new InvalidOperationException($"{nameof(RealLightsSpec)} for prefab {name} has no lights");
                }

                dict[name] = spec;
            }
        }

        realLightSpecs = dict.ToFrozenDictionary();

        eb.Register(this);
    }

    public RealLightsSpec? GetRealLightFor(PrefabSpec prefab) => TryGetRealLightFor(prefab, out var spec) ? spec : null;

    public bool TryGetRealLightFor(PrefabSpec prefab, [MaybeNullWhen(false)] out RealLightsSpec spec) => realLightSpecs.TryGetValue(prefab.PrefabName, out spec);

    public void Register(RealLightsComponent comp)
    {
        instances.Add(comp);
    }

    public void Unregister(RealLightsComponent comp)
    {
        instances.Remove(comp);
    }

    [OnEvent]
    public void OnNighttime(NighttimeStartEvent _)
    {
        foreach (var i in instances)
        {
            i.UpdateDayNightLights();
        }
    }

    [OnEvent]
    public void OnDaytime(DaytimeStartEvent _)
    {
        foreach (var i in instances)
        {
            i.UpdateDayNightLights();
        }
    }

}
