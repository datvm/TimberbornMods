
namespace BuildingHP.Components.Renovations;

public class BuildingReinforcementComponent : BaseComponent, IBuildingDeltaDurabilityModifier, IActiveRenovationDescriber
{
    const string NoReinforcementKey = "LV.BHP.NoReinforcement";
    static readonly FrozenSet<string> BasicReinforcementId = ["Reinforce1", "Reinforce2", "Reinforce3"];

    public string DescriptionKey { get; private set; } = NoReinforcementKey;
    public int? Delta { get; private set; }
    public float? ModifierEndTime { get; }

    public event Action<IBuildingDurabilityModifier>? OnChanged;

    public void Awake()
    {
        var reno = this.GetRenovationComponent();
        reno.RenovationCompleted += OnRenovationCompleted;
    }

    void OnRenovationCompleted(BuildingRenovation obj)
    {
        if (!BasicReinforcementId.Contains(obj.Id)) { return; }
        LookForReinforcement();
    }

    /// <summary>
    /// Looks for the best reinforcement renovation and applies it.
    /// </summary>
    void LookForReinforcement()
    {
        var comp = this.GetRenovationComponent();
        var serv = comp.RenovationService;
        var activeIds = BasicReinforcementId.Where(comp.ActiveRenovations.Contains).ToArray();

        if (activeIds.Length == 0) { return; }

        var topSpec = serv.GetSpec(activeIds[0]);
        if (activeIds.Length > 1)
        {
            for (int i = 1; i < activeIds.Length; i++)
            {
                var spec = serv.GetSpec(activeIds[i]);

                if (topSpec.Parameters[0] < spec.Parameters[0])
                {
                    topSpec = spec;
                }
            }

            // Remove all but the top one from the active list
            foreach (var id in activeIds)
            {
                if (id != topSpec.Id)
                {
                    comp.RemoveActiveRenovation(id);
                }
            }
        }

        SetReinforcement((int)topSpec.Parameters[0], topSpec.TitleLoc);
    }

    public void SetReinforcement(int? delta, string? descriptionKey = null)
    {
        Delta = delta;
        DescriptionKey = descriptionKey ?? NoReinforcementKey;

        OnChanged?.Invoke(this);
    }

    public void Initialize()
    {
        LookForReinforcement();
    }

    public ActiveRenovationDescription? Describe(ILoc t, IDayNightCycle dayNightCycle)
        => Delta is null or 0 ? null : new(t.T(DescriptionKey), t.T("LV.BHP.ReinforceEff", Delta));
}
