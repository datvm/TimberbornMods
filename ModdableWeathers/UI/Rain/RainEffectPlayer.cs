namespace ModdableWeathers.UI.Rain;

public class RainEffectPlayer(
    RainSettings settings,
    WindService windService,
    MapSize mapSize,
    EventBus eb
)
    : ModdableWeatherPriorityModifierServiceBase<RainEffectModifier>, ILoadableSingleton
{
    static readonly RainEffectModifier DefaultModifier
        = new(IWeatherEntityModifierEntry.DefaultId, int.MinValue, null);
    public override RainEffectModifier Default { get; } = DefaultModifier;

    const float WindStrengthModifier = 5f;
    const float ParticleLifetime = 2.5f;

    public bool CanRain { get; private set; }
    public bool IsRaining { get; private set; }

    GameObject? rainObj;
    ParticleSystem? ps;
    Material? material;

    public void Load()
    {
        CanRain = settings.RainEnabled;
        if (!CanRain) { return; }

        AttachRain();
        eb.Register(this);
        OnWindChanged(null!);

        settings.OnSettingsChanged += OnSettingsChanged;
    }

    void OnSettingsChanged()
    {
        if (!CanRain) { return; }

        CanRain = settings.RainEnabled;
        if (!CanRain)
        {
            DisableRain();
            return;
        }

        var intensity = settings.RainIntensity;
        var maxParticles = settings.MaxRainParticles;

        var emission = ps!.emission;
        var size = mapSize.TotalSize;
        emission.rateOverTime = Math.Min(maxParticles, (int)(intensity * size.x * size.y));

        var main = ps.main;
        main.maxParticles = Mathf.CeilToInt(maxParticles * ParticleLifetime * 2);
    }

    protected override void ActivateNewModifier()
    {
        base.ActivateNewModifier();

        var active = ActiveEntry;
        if (active.RainColor is null)
        {
            StopRain();
        }
        else
        {
            StartRain(active.RainColor.Value);
        }
    }

    void StartRain(Color color)
    {
        if (!rainObj || !CanRain) { return; }

        if (IsRaining)
        {
            StopRain();
        }

        material!.SetColor("_BaseColor", color);

        IsRaining = true;
        rainObj.SetActive(true);
    }

    void StopRain()
    {
        if (!rainObj || !CanRain || !IsRaining) { return; }

        IsRaining = false;
        rainObj.SetActive(false);
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
        main.maxParticles = Mathf.CeilToInt(settings.MaxRainParticles * ParticleLifetime * 2);

        var size = mapSize.TotalSize;

        var emission = ps.emission;
        emission.rateOverTime = Math.Min(settings.MaxRainParticles, (int)(settings.RainIntensity * size.x * size.y));

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

    void DisableRain()
    {
        if (!rainObj) { return; }

        GameObject.Destroy(material);
        GameObject.Destroy(rainObj);

        rainObj = null;
        ps = null;
        material = null;
    }

    [OnEvent]
    public void OnWindChanged(WindChangedEvent _)
    {
        SetWindDirection(windService.WindDirection, windService.WindStrength);
    }

    public void SetWindDirection(Vector2 direction, float strength)
    {
        if (!rainObj || !CanRain) { return; }

        var velocity = ps!.velocityOverLifetime;
        strength *= WindStrengthModifier;
        velocity.x = direction.x * strength;
        velocity.z = direction.y * strength;
    }

}
