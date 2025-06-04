namespace ModdableWeather.UI;

public class RainEffect(
    RainWeatherSettings s,
    EventBus eb,
    WindService windService,
    MapSize mapSize,
    RainWeather rainWeather
) : ILoadableSingleton
{
    const float RainParticlePerSquare = 200f / (32 * 32);
    const float WindStrengthModifier = 5f;

    public bool CanRain { get; private set; }
    public bool IsRaining { get; private set; }

    GameObject? rainObj;
    ParticleSystem ps = null!;

    public void Load()
    {
        CanRain = s.EnableRainEffect.Value;
        if (!CanRain) { return; }

        AttachRain();
        eb.Register(this);
        OnWindChanged(null!);

        EnableRain();
        rainWeather.OnWeatherActiveChanged += RainWeather_OnWeatherActiveChanged;
    }

    private void RainWeather_OnWeatherActiveChanged(IModdedWeather weather, bool active, bool onLoad)
    {
        if (active)
        {
            EnableRain();
        }
        else
        {
            DisableRain();
        }
    }

    public void EnableRain()
    {
        if (!rainObj || !CanRain || IsRaining) { return; }

        IsRaining = true;
        rainObj.SetActive(true);
    }

    public void DisableRain()
    {
        if (!rainObj || !CanRain || !IsRaining) { return; }

        IsRaining = false;
        rainObj.SetActive(false);
    }

    public void SetWindDirection(Vector2 direction, float strength)
    {
        if (!rainObj || !CanRain) { return; }

        var velocity = ps.velocityOverLifetime;
        strength *= WindStrengthModifier;
        velocity.x = direction.x * strength;
        velocity.z = direction.y * strength;
    }

    void AttachRain()
    {
        rainObj = new GameObject("RainEffect");
        rainObj.SetActive(false);

        ps = rainObj.AddComponent<ParticleSystem>();

        var shader = Shader.Find("Universal Render Pipeline/Unlit");
        var material = new Material(shader);
        material.SetColor("_BaseColor", new Color(0.5f, 0.5f, 1f, 0.4f));
        material.SetFloat("_Surface", 1);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

        var main = ps.main;
        main.startSpeed = 0f;
        main.startSize3D = true;
        main.startSizeX = new(.05f / 2f, .1f / 2f);
        main.startSizeY = new(.2f /2f, .4f /2f);
        main.startSizeZ = 1f;

        main.startLifetime = 2.5f;
        main.gravitySource = ParticleSystemGravitySource.Physics3D;
        main.gravityModifier = 2f;
        main.loop = true;
        main.maxParticles = 1_000_000;

        var size = mapSize.TotalSize;

        var emission = ps.emission;
        emission.rateOverTime = (int)(RainParticlePerSquare * size.x * size.y);

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new(size.x, 1, size.y);
        rainObj.transform.position = new(size.x / 2, size.z + 1, size.y / 2);

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;

        var collision = ps.collision;
        collision.type = ParticleSystemCollisionType.Planes;
        collision.AddPlane(CreateCollisionPlane());
        collision.lifetimeLoss = 1f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = material;
    }

    Transform CreateCollisionPlane()
    {
        var obj = new GameObject();
        obj.transform.localScale = new(1E6f, 1, 1E6f);

        return obj.transform;
    }

    [OnEvent]
    public void OnWindChanged(WindChangedEvent _)
    {
        SetWindDirection(windService.WindDirection, windService.WindStrength);
    }

}