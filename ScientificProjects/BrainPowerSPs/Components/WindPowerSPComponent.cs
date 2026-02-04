namespace BrainPowerSPs.Components;

public class WindPowerSPComponent : BaseComponent, IEntityEffectDescriber, IStartableComponent, IModdableMechanicalNodeModifier
{

    WindPowerSPService service = null!;

    public event Action? OnChanged;

    public int HighestZ { get; private set; }
    public float HeightMultiplier { get; private set; } = 1;
    public string Id { get; } = nameof(WindPowerSPComponent);
    public int Priority { get; } = (int)ModifierPriority.Multiplicative;
    public bool Disabled { get; private set; } = true;

    [Inject]
    public void Inject(WindPowerSPService service)
    {
        this.service = service;
    }

    public void Start()
    {
        var blockObj = GetComponent<BlockObject>();
        HighestZ = blockObj.Coordinates.z + blockObj.Blocks.Size.z;

        CalculateBoost();
    }

    public void CalculateBoost()
    {
        var level = service.BoostPerLevel;
        var shouldDisable = level == 0;

        if (shouldDisable)
        {
            if (!Disabled)
            {
                HeightMultiplier = 1;
                Disabled = true;
                OnChanged!.Invoke();
            }
        }
        else
        {
            Disabled = false;
            HeightMultiplier = 1 + service.BoostPerLevel * HighestZ;
            OnChanged!.Invoke();
        }   
    }

    public EntityEffectDescription? Describe(ILoc t, IDayNightCycle dayNightCycle) 
        => HeightMultiplier > 1
            ? new(
                t.T("LV.BPSP.WindmillHeightUp"),
                t.T("LV.BPSP.WindmillHeightUpDesc", HeightMultiplier - 1f, HighestZ))
            : null;

    public bool Modify(ModdableMechanicalNodeValues value)
    {
        value.Value = value.Value with
        {
            NominalOutput = Mathf.FloorToInt(value.Value.NominalOutput * HeightMultiplier),
        };

        return false;
    }
}
