
namespace BrainPowerSPs.Components;

public class WindPowerSPComponent : BaseComponent, IEntityEffectDescriber, IStartableComponent
{

    WindPowerSPService service = null!;

    public int HighestZ { get; private set; }
    public float HeightMultiplier { get; private set; } = 1;

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
        HeightMultiplier = 1 + service.BoostPerLevel * HighestZ;
    }

    public EntityEffectDescription? Describe(ILoc t, IDayNightCycle dayNightCycle) 
        => HeightMultiplier > 1
            ? new(
                t.T("LV.BPSP.WindmillHeightUp"),
                t.T("LV.BPSP.WindmillHeightUpDesc", HeightMultiplier - 1f, HighestZ))
            : null;
}
