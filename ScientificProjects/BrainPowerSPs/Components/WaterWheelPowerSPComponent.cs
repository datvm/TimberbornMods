namespace BrainPowerSPs.Components;

public class WaterWheelPowerSPComponent : BaseComponent, IModdableMechanicalNodeModifier, IEntityMultiEffectsDescriber, IAwakableComponent
{
    public string Id { get; } = nameof(WaterWheelPowerSPComponent);
    public int Priority { get; } = (int)ModifierPriority.Multiplicative;
    public bool Disabled { get; }

    public event Action? OnChanged;
    public float MinimumGeneratorStrength { get; set; }

#nullable disable
    WaterWheelPowerSPService service;
    WaterPoweredGenerator waterPoweredGenerator;
#nullable enable

    public IReadOnlyList<Vector3Int> GroundedCoordinates => waterPoweredGenerator._groundedCoordinates;

    [Inject]
    public void Inject(WaterWheelPowerSPService service)
    {
        this.service = service;
    }

    public void Awake()
    {
        waterPoweredGenerator = GetComponent<WaterPoweredGenerator>();
    }

    public bool Modify(ModdableMechanicalNodeValues value)
    {
        value.Value = value.Value with
        {
            NominalOutput = Mathf.FloorToInt(value.Value.NominalOutput * service.CurrentModifier),
        };

        return false;
    }

    public void OnModifierChanged() => OnChanged!.Invoke();

    public IEnumerable<EntityEffectDescription> DescribeAll(ILoc t, IDayNightCycle dayNightCycle)
    {
        var powerUp = service.ActivePowerUpProject;
        if (powerUp is not null)
        {
            yield return new(
                powerUp.Spec.DisplayName,
                t.T("LV.BPSP.WaterWheelUpBuffDesc", service.CurrentModifier - 1f)
            );
        }

        if (service.ActiveFlowUpProjects.Count > 0)
        {
            var sum = 0f;
            var desc = "";

            foreach (var flowUp in service.ActiveFlowUpProjects)
            {
                sum += flowUp.GetEffect(0);
                desc += Environment.NewLine + flowUp.DescribeEffect(t, 0, true);
            }


            yield return new(
                t.T("LV.BPSP.WaterWheelFlowUp1"),
                t.T("LV.BPSP.WaterWheelFlowUpBuffDesc", sum) + desc
            );
        }        
    }
}
