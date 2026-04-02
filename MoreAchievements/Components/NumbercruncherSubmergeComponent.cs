namespace MoreAchievements.Components;

[AddTemplateModule2(typeof(NumbercruncherFloodSpec))]
public class NumbercruncherSubmergeComponent : BaseComponent, IAwakableComponent
{

    WaterObject waterObject = null!;
    int height;

    public event EventHandler? OnSubmerged;

    public void Awake()
    {
        waterObject = GetComponent<WaterObject>();
        height = GetComponent<BlockObjectSpec>().Size.z;
    }

    public void Enable()
    {
        waterObject.WaterAboveBaseChanged += OnWaterAboveBaseChanged;
        EnableComponent();
    }

    public void Disable()
    {
        waterObject.WaterAboveBaseChanged -= OnWaterAboveBaseChanged;
        DisableComponent();
    }

    void OnWaterAboveBaseChanged(object sender, EventArgs e)
    {
        if (OnSubmerged is null) { return; }

        if (waterObject.WaterAboveBase >= height)
        {
            OnSubmerged(this, EventArgs.Empty);
        }
    }

}
