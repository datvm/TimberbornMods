namespace Ziporter.Components;

public class ZiporterLighting : BaseComponent, IFinishedStateListener
{
    const float MaxIntensity = 3f;
    const float IntensityRate = 1f;

    static readonly int[] Red = [1, 0, 0];
    static readonly int[] Green = [0, 1, 0];
    bool fadingOut = true;

    Light light = null!;
    ZiporterBattery battery = null!;

    public ZiporterLighting()
    {
        enabled = false;
    }

    public void Awake()
    {
        battery = GetComponentFast<ZiporterBattery>();
    }

    public void Start()
    {
        AttachLight();
    }

    public void Update()
    {
        if (!light) { return; }

        UpdateStatusLight();
    }

    void UpdateStatusLight()
    {
        SetLightColor();

        var change = Time.deltaTime * IntensityRate * (fadingOut ? -1 : 1);
        light.intensity = Mathf.Clamp(light.intensity + change, 0, MaxIntensity);

        if (light.intensity <= 0)
        {
            fadingOut = false;
        }
        else if (light.intensity >= MaxIntensity)
        {
            fadingOut = true;
        }
    }

    void SetLightColor()
    {
        var perc = battery.Charge / battery.Capacity;

        var colorParts = Green.Zip(Red, (g, r) => g * perc + r * (1 - perc)).ToArray();
        light.color = new Color(colorParts[0], colorParts[1], colorParts[2]);
    }

    public void OnDestroy()
    {
        if (light)
        {
            Destroy(light);
        }
    }

    void AttachLight()
    {
        var lightObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lightObj.GetComponent<Renderer>().enabled = false;
        lightObj.transform.localScale = new(.3f, .3f, .3f);
        lightObj.transform.parent = TransformFast;

        light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 3f;

        light.transform.localPosition = CoordinateSystem.GridToWorld(new Vector3(1.5f, .5f, 1.5f));
        light.intensity = MaxIntensity;
        light.color = Color.green;
        light.enabled = enabled;
    }

    public void OnEnterFinishedState()
    {
        enabled = true;
        if (light) { light.enabled = true; }
    }

    public void OnExitFinishedState()
    {
        enabled = false;
        if (light) { light.enabled = false; }
    }
}
