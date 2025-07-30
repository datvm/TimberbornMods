namespace ModdableWeather.UI;

public class RainEffect(
    RainWeatherSettings s,
    EventBus eb,
    WindService windService,
    MapSize mapSize,
    IEnumerable<IRainEffectWeather> rainWeathers
) : ILoadableSingleton
{
    const float RainParticlePerSquare = 200f / (32 * 32);
    const int MaxParticleRate = 1000;
    const float WindStrengthModifier = 5f;
    const float ParticleLifetime = 2.5f;

    public bool CanRain { get; private set; }
    public bool IsRaining { get; private set; }

    GameObject? rainObj;
    ParticleSystem ps = null!;
    Material material = null!;

    public void Load()
    {
        CanRain = s.EnableRainEffect.Value;
        if (!CanRain) { return; }

        AttachRain();
        eb.Register(this);
        OnWindChanged(null!);

        foreach (var w in rainWeathers)
        {
            w.OnWeatherActiveChanged += (_, active, onLoad) => OnWeatherActiveChanged(w, active, onLoad);
            if (w.Active)
            {
                StartRain(w.RainColor);
            }
        }
    }

    private void OnWeatherActiveChanged(IRainEffectWeather weather, bool active, bool onLoad)
    {
        if (active)
        {
            StartRain(weather.RainColor);
        }
        else
        {
            StopRain();
        }
    }

    public void StartRain(Color color)
    {
        material.SetColor("_BaseColor", color);
        EnableRain();
    }

    public void StopRain()
    {
        DisableRain();
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
        material = new Material(shader);
        material.SetColor("_BaseColor", new Color(0.5f, 0.5f, 1f, 0.4f));
        material.SetFloat("_Surface", 1);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

        var main = ps.main;
        main.startSpeed = 0f;
        main.startSize3D = true;
        main.startSizeX = new(.025f, .05f);
        main.startSizeY = new(.1f, .2f);
        main.startSizeZ = new(1f, 1f);

        main.startLifetime = ParticleLifetime;
        main.gravitySource = ParticleSystemGravitySource.Physics3D;
        main.gravityModifier = 1f;
        main.loop = true;
        main.maxParticles = Mathf.CeilToInt(MaxParticleRate * ParticleLifetime * 2);

        var size = mapSize.TotalSize;

        var emission = ps.emission;
        emission.rateOverTime = Math.Min(MaxParticleRate, (int)(RainParticlePerSquare * size.x * size.y));

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new(size.x, 1, size.y);
        rainObj.transform.position = new(size.x / 2, size.z + 1, size.y / 2);

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = -10f;

        var collision = ps.collision;
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        //collision.AddPlane(CreateCollisionPlane());
        collision.lifetimeLoss = 1f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = material;
    }

    [OnEvent]
    public void OnWindChanged(WindChangedEvent _)
    {
        SetWindDirection(windService.WindDirection, windService.WindStrength);
    }

}