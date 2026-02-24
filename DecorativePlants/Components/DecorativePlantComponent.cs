namespace DecorativePlants.Components;

public record DecorativePlantSpec : ComponentSpec;

[AddTemplateModule2(typeof(DecorativePlantSpec))]
public class DecorativePlantComponent : BaseComponent, IDuplicable<DecorativePlantComponent>, IAwakableComponent, IStartableComponent, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(DecorativePlantComponent));
    static readonly PropertyKey<int> MatureStateKey = new("MatureState");
    static readonly PropertyKey<int> WellnessStateKey = new("WellnessState");

    public static readonly ImmutableArray<PlantMatureState> AllMatureStates = GetAllEnums<PlantMatureState>();
    public static readonly ImmutableArray<PlantWellnessState> AllWellnessStates = GetAllEnums<PlantWellnessState>();

    public PlantMatureState MatureState { get; private set; } = PlantMatureState.Mature;
    public PlantWellnessState WellnessState { get; private set; } = PlantWellnessState.Alive;

    public void Awake()
    {
        UpdateModel();
    }

    public void Start()
    {
        UpdateModel();
    }

    public void SetState(PlantMatureState? mature = null, PlantWellnessState? wellness = null)
    {
        if (mature.HasValue)
        {
            MatureState = mature.Value;
        }

        if (wellness.HasValue)
        {
            WellnessState = wellness.Value;
        }

        UpdateModel();
    }

    public void DuplicateFrom(DecorativePlantComponent source)
    {
        MatureState = source.MatureState;
        WellnessState = source.WellnessState;
        UpdateModel();
    }

    void UpdateModel()
    {
        var currMaturity = (int)MatureState;
        var currWellness = (int)WellnessState;

        var models = GetModels();
        for (int i = 0; i < AllMatureStates.Length; i++)
        {
            for (int j = 0; j < AllWellnessStates.Length; j++)
            {
                models[i][j].SetActive(i == currMaturity && j == currWellness);
            }
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(MatureStateKey, (int)MatureState);
        s.Set(WellnessStateKey, (int)WellnessState);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        MatureState = (PlantMatureState)s.Get(MatureStateKey);
        WellnessState = (PlantWellnessState)s.Get(WellnessStateKey);
    }

    GameObject[][] GetModels()
    {
        GameObject[][] models = new GameObject[AllMatureStates.Length][];

        foreach (var m in AllMatureStates)
        {
            var matureObj = GameObject.FindChild(m.ToString());
            var wellnessObjs = models[(int)m] = new GameObject[AllWellnessStates.Length];

            foreach (var w in AllWellnessStates)
            {
                wellnessObjs[(int)w] = matureObj.FindChild("#" + w.ToString());
            }
        }

        return models;
    }

    static ImmutableArray<T> GetAllEnums<T>()
        => [.. Enum.GetValues(typeof(T)).Cast<T>().OrderBy(q => q)];

}
