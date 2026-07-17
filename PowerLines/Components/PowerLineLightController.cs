namespace PowerLines.Components;

[AddTemplateModule2(typeof(PowerLineLightControllerSpec))]
public class PowerLineLightController : TickableComponent, IAwakableComponent, IFinishedStateListener
{
    // Dim warm → bright warm white as efficiency rises
    static readonly Color DimColor = new(1f, 0.45f, 0.12f);
    static readonly Color BrightColor = new(1f, 0.95f, 0.75f);

    const float MinStrength = 0.3f;
    const float MaxStrength = 1.75f;
    const int ColorPriority = 50;

    MechanicalNode mechNode = null!;
    Illuminator illuminator = null!;

    IlluminatorToggle? toggle;
    IlluminatorColorizer? colorizer;

    bool wasOn;
    float lastStrength = -1f;
    Color lastColor;

    public void Awake()
    {
        mechNode = GetComponent<MechanicalNode>();
        illuminator = GetComponent<Illuminator>();
    }

    public void OnEnterFinishedState()
    {
        toggle = illuminator.CreateToggle();
        colorizer = illuminator.CreateColorizer(ColorPriority);
        wasOn = false;
        lastStrength = -1f;
        Apply(force: true);
    }

    public void OnExitFinishedState()
    {
        toggle?.TurnOff();
        colorizer?.ClearColor();
        toggle = null;
        colorizer = null;
        wasOn = false;
        lastStrength = -1f;
    }

    public override void Tick() => Apply(false);

    void Apply(bool force)
    {
        var powered = mechNode.ActiveAndPowered;
        var efficiency = powered ? mechNode.PowerEfficiency : 0f;

        if (force || powered != wasOn)
        {
            wasOn = powered;
            if (powered)
            {
                toggle!.TurnOn();
            }
            else
            {
                toggle!.TurnOff();
                lastStrength = -1f;
                return;
            }
        }

        if (!powered) { return; }

        var strength = Mathf.Lerp(MinStrength, MaxStrength, efficiency);
        if (force || !Mathf.Approximately(strength, lastStrength))
        {
            lastStrength = strength;
            illuminator.SetStrength(strength);
        }

        var color = Color.Lerp(DimColor, BrightColor, efficiency);
        if (force || lastColor != color)
        {
            lastColor = color;
            colorizer!.SetColor(color);
        }
    }
}
