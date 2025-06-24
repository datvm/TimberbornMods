namespace HydroFormaProjects.Components;

public class FloodgateAutoComponent : BaseComponent, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new("FloodgateAuto");
    static readonly PropertyKey<bool> SetOnHazardKey = new("SetOnHazard");
    static readonly PropertyKey<float> HeightOnHazardKey = new("HeightOnHazard");
    static readonly PropertyKey<bool> SetOnNewCycleKey = new("SetOnNewCycle");
    static readonly PropertyKey<float> HeightOnNewCycleKey = new("HeightOnNewCycle");

#nullable disable
    Floodgate floodgate;
#nullable enable

    public int MaxHeight => floodgate.MaxHeight;

    public bool SetOnHazard { get; set; }
    public float HeightOnHazard
    {
        get;
        set => SetEventHeight(ref field, value);
    }
    public bool SetOnNewCycle { get; set; }
    public float HeightOnNewCycle
    {
        get;
        set => SetEventHeight(ref field, value);
    }

    public void Awake()
    {
        floodgate = GetComponentFast<Floodgate>();
    }

    public void Start()
    {
        HeightOnHazard = Mathf.Clamp(HeightOnHazard, 0, MaxHeight);
        HeightOnNewCycle = Mathf.Clamp(HeightOnNewCycle, 0, MaxHeight);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s))
        {
            return;
        }

        if (s.Has(SetOnHazardKey))
        {
            SetOnHazard = s.Get(SetOnHazardKey);
        }
        if (s.Has(HeightOnHazardKey))
        {
            HeightOnHazard = s.Get(HeightOnHazardKey);
        }
        if (s.Has(SetOnNewCycleKey))
        {
            SetOnNewCycle = s.Get(SetOnNewCycleKey);
        }
        if (s.Has(HeightOnNewCycleKey))
        {
            HeightOnNewCycle = s.Get(HeightOnNewCycleKey);
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        if (SetOnHazard)
        {
            s.Set(SetOnHazardKey, SetOnHazard);
        }
        s.Set(HeightOnHazardKey, HeightOnHazard);

        if (SetOnNewCycle)
        {
            s.Set(SetOnNewCycleKey, SetOnNewCycle);
        }
        s.Set(HeightOnNewCycleKey, HeightOnNewCycle);
    }

    public void SetHeight(float height) => floodgate.SetHeightAndSynchronize(Mathf.Clamp(height, 0, MaxHeight));

    void SetEventHeight(ref float target, float height) => target = Mathf.Clamp(height, 0, MaxHeight);

}
