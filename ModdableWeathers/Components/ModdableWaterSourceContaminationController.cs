namespace ModdableWeathers.Components;

public class ModdableWaterSourceContaminationController(ModdableWaterContaminationModifierService service)
    : TickableComponent, IAwakableComponent
{

#nullable disable
    WaterSourceContamination contamination;
    WaterSourceContaminationSpec spec;
#nullable enable

    public void Awake()
    {
        contamination = GetComponent<WaterSourceContamination>();
        spec = GetComponent<WaterSourceContaminationSpec>();

        service.OnTickingChanged += OnTickingChanged;
    }

    public override void Tick()
    {
        var modifier = service.CurrentModifier;
        var value = Mathf.Clamp01(spec.DefaultContamination + modifier);

        contamination.SetContamination(value);
    }

    void OnTickingChanged(bool ticking)
    {
        if (ticking)
        {
            EnableComponent();
        }
        else
        {
            DisableComponent();
        }
    }
}
