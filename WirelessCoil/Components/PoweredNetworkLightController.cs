namespace WirelessCoil.Components;

public class PoweredNetworkLightController : TickableComponent, IFinishedStateListener
{
    static readonly Vector3 RedLight = new(1f, .2f, .2f);
    static readonly Vector3 GreenLight = new(0f, 1f, 0f);

    BuildingLightToggle? buildingLightToggle;
    MechanicalNode? mechanicalNode;
    bool isOn;
    Light light = null!;

    public void Awake()
    {
        light = GameObjectFast.GetComponentInChildren<Light>();
    }

    public void OnEnterFinishedState()
    {
        buildingLightToggle = GetComponentFast<BuildingLighting>()?.GetBuildingLightToggle();
        mechanicalNode = GetComponentFast<MechanicalNode>();
    }

    public void OnExitFinishedState()
    {
        buildingLightToggle = null;
        mechanicalNode = null;
    }

    public override void Tick()
    {
        if (buildingLightToggle is null || !mechanicalNode) { return; }

        var shouldBeOn = mechanicalNode.Graph.Powered;
        if (isOn != shouldBeOn)
        {
            isOn = shouldBeOn;
            if (shouldBeOn)
            {
                buildingLightToggle.TurnOn();
            }
            else
            {
                buildingLightToggle.TurnOff();
            }
        }

        if (light)
        {
            var power = mechanicalNode.Graph.CurrentPower;
            var eff = power.PowerSupply == 0 ? 0 : mechanicalNode.Graph.CurrentPower.PowerEfficiency;
            var color = new Color(
                Mathf.Lerp(RedLight.x, GreenLight.x, eff),
                Mathf.Lerp(RedLight.y, GreenLight.y, eff),
                Mathf.Lerp(RedLight.z, GreenLight.z, eff)
            );

            light.color = color;
        }
    }
}
