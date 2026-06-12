namespace ModdableTimberborn.ModdableWaterSource;

public class ModdableWaterSourceComponent : TickableComponent, IAwakableComponent, IWaterStrengthModifier
{

    public WaterSource WaterSource { get; private set; } = null!;
    public WaterSourceContamination WaterSourceContamination { get; private set; } = null!;

    public float DefaultContamination { get; private set; }
    public bool IsContaminationSource { get; private set; }

    public StackableValue<bool> ImmuneToDrought { get; set; } = new(false);
    public StackableValue<bool> ImmuneToBadtide { get; set; } = new(false);

    public StackableValue<float> StrengthMultiplier { get; set; } = new(1f);
    public StackableValue<float> ContaminationDelta { get; set; } = new(0f);
    float prevContaminationDelta = 0f;

    public void Awake()
    {
        WaterSource = GetComponent<WaterSource>();
        WaterSourceContamination = GetComponent<WaterSourceContamination>();

        DefaultContamination = WaterSourceContamination._waterSourceContaminationSpec.DefaultContamination;
        IsContaminationSource = DefaultContamination > 0f;

        WaterSource.AddWaterStrengthModifier(this);
    }

    public float GetStrengthModifier() => StrengthMultiplier.CalculateThisFrame();

    public override void Tick()
    {
        var contaminationValue = ContaminationDelta.CalculateThisFrame();
        if (prevContaminationDelta != contaminationValue)
        {
            WaterSourceContamination.SetContamination(Mathf.Clamp01(DefaultContamination + contaminationValue));
            prevContaminationDelta = contaminationValue;
        }
    }

}