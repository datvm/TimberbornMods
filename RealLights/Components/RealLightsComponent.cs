﻿global using Newtonsoft.Json;
global using Timberborn.Coordinates;
global using Timberborn.PrefabSystem;
global using Timberborn.TimeSystem;

namespace RealLights.Components;

public class RealLightsComponent : BaseComponent, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new("BuildingRealLights");
    static readonly PropertyKey<bool> ForceNightLightOnKey = new("ForceNightLightOn");
    static readonly PropertyKey<string> CustomProperties = new("CustomProperties");

    ImmutableArray<Light> lights = default;
    RealLightsManager registry = null!;
    IDayNightCycle dayNightCycle = null!;

    public bool HasNightLight { get; private set; }
    public bool ForceNightLightOn { get; private set; }

    public RealLightsSpec? Spec { get; private set; }
    public bool HasRealLight => Spec is not null;

    CustomRealLightProperties[]? customs;

    public bool IsDrawingDebugLight { get; private set; }

    public Vector3 FirstLightPosition => CoordinateSystem.WorldToGrid(lights[0].transform.localPosition);

    [Inject]
    public void Inject(RealLightsManager registry, IDayNightCycle dayNightCycle)
    {
        this.registry = registry;
        this.dayNightCycle = dayNightCycle;
    }

    public void Start()
    {
        var prefabSpec = GetComponentFast<PrefabSpec>();
        if (prefabSpec is null
            || !registry.TryGetRealLightFor(prefabSpec, out var spec)) { return; }
        Spec = spec;

        AttachLights(spec.Lights, TransformFast);
        HasNightLight = spec.Lights.FastAny(q => q.IsNightLight);

        registry.Register(this);
        UpdateAllLights();
        ToggleLightsState();
    }

    public void OnDestroy()
    {
        registry.Unregister(this);

        if (lights != default)
        {
            foreach (var l in lights)
            {
                Destroy(l.gameObject);
            }
        }
    }

    void AttachLights(ImmutableArray<RealLightLightSpec> specs, Transform parent)
    {
        if (lights != default) { return; }

        List<Light> newLights = [];
        foreach (var spec in specs)
        {
            var lightObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lightObj.GetComponent<Renderer>().enabled = false;
            lightObj.transform.localScale = new(.3f, .3f, .3f);
            lightObj.transform.parent = parent;

            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;

            light.transform.localPosition = CoordinateSystem.GridToWorld(spec.Position);
            light.enabled = false;

            newLights.Add(light);
        }

        lights = [.. newLights];
    }

    public void Reset()
    {
        if (lights == default) { return; }
        customs = null;
        SetLightPosition(Spec!.Lights[0].Position);
        ForceNightLightOn = false;

        UpdateAllLights();
    }

    public void SetCustomProperties(int index, CustomRealLightProperties props)
    {
        if (Spec is null) { throw new InvalidOperationException("This object has no Real Light"); }

        customs ??= new CustomRealLightProperties[Spec.Lights.Length];
        customs[index] = customs[index].Modify(props);

        UpdateLight(index);
    }

    public void SetCustomColor(int index, Func<Color, Color> modifier)
    {
        if (Spec is null) { throw new InvalidOperationException("This object has no Real Light"); }

        var currColor = GetLightProperties(index).Color;
        var modified = modifier(currColor);

        SetCustomProperties(index, new() { Color = modified });
    }

    public void SetForceNightLightOn(bool value)
    {
        ForceNightLightOn = value;
        ToggleLightsState();
    }

    public void SetLightPosition(Vector3 position)
    {
        if (lights == default) { return; }
        lights[0].transform.localPosition = CoordinateSystem.GridToWorld(position);
    }

    public void ToggleDebugLight(bool enabled)
    {
        if (lights == default) { return; }

        IsDrawingDebugLight = enabled;
        foreach (var l in lights)
        {
            l.GetComponent<Renderer>().enabled = enabled;
        }
    }

    public RealLightProperties GetLightProperties(int index) => Spec is null
        ? throw new InvalidOperationException("This object has no Real Light")
        : ((RealLightProperties)Spec.Lights[index]).Modify(customs?[index]);

    public void UpdateDayNightLights()
    {
        if (!HasNightLight || ForceNightLightOn) { return; }

        ToggleLightsState();
    }

    public void UpdateShadows(LightShadows shadows)
    {
        if (lights == default) { return; }

        foreach (var l in lights)
        {
            l.shadows = shadows;
        }
    }

    void UpdateAllLights()
    {
        if (lights == default) { return; }
        for (int i = 0; i < lights.Length; i++)
        {
            UpdateLight(i);
        }
    }

    void UpdateLight(int index)
    {
        if (lights == default) { throw new InvalidOperationException("Lights not initialized"); }

        var light = lights[index];
        var (color, range, intensity) = GetLightProperties(index);
        
        light.range = range;
        light.color = color;
        light.intensity = intensity;
        light.enabled = ShouldLightsBeOn;
    }

    void ToggleLightsState()
    {
        if (lights == default || Spec is null) { return; }

        var lightSpecs = Spec.Lights;

        var shouldBeOn = ShouldLightsBeOn;
        for (int i = 0; i < lights.Length; i++)
        {
            var l = lights[i];
            l.enabled = shouldBeOn || !lightSpecs[i].IsNightLight;
        }
    }

    public bool ShouldLightsBeOn => !HasNightLight || ForceNightLightOn || dayNightCycle.IsNighttime;

    static readonly ColorHandler jsonColorHandler = new();
    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);

        if (ForceNightLightOn)
        {
            s.Set(ForceNightLightOnKey, ForceNightLightOn);
        }

        if (customs is not null)
        {
            s.Set(CustomProperties, JsonConvert.SerializeObject(customs, jsonColorHandler));
        }
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.HasComponent(SaveKey)) { return; }

        var s = entityLoader.GetComponent(SaveKey);

        ForceNightLightOn = s.Has(ForceNightLightOnKey) && s.Get(ForceNightLightOnKey);

        if (s.Has(CustomProperties))
        {
            customs = JsonConvert.DeserializeObject<CustomRealLightProperties[]>(s.Get(CustomProperties), jsonColorHandler);
        }
    }

}

