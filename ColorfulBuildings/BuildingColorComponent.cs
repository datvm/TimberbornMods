global using Timberborn.EntitySystem;

namespace ColorfulBuildings;

public class BuildingColorComponent : BaseComponent, IPersistentEntity, IDeletableEntity
{
    static readonly ComponentKey SaveKey = new("BuildingColor");
    static readonly PropertyKey<Vector3Int> ColorKey = new("Color");

    public Vector3Int? Color { get; private set; }
    Color? originalColor;

    public Renderer? Renderer { get; private set; }

    public void SetColor(Vector3Int color)
    {
        var material = Renderer?.material;
        if (material is null) { return; }

        Color = color;
        var c = new Color(color.x / 255f, color.y / 255f, color.z / 255f);
        SetColorToMaterial(material, c);
    }

    public void ClearColor()
    {
        Color = null;
        
        var material = Renderer?.material;
        if (material is null) { return; }

        ClearColorToMaterial(material);
    }

    void SetColorToMaterial(Material material, Color color)
    {
        originalColor ??= material.color;
        material.color = color;
    }

    void ClearColorToMaterial(Material material)
    {
        if (originalColor is null) { return; }
        material.color = originalColor.Value;
    }

    public void Awake()
    {
        Renderer = GetComponentInChildren<Renderer>(true);
    }

    public void Start()
    {
        if (Color is not null)
        {
            SetColor(Color.Value);
        }
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.HasComponent(SaveKey)) { return; }

        var s = entityLoader.GetComponent(SaveKey);

        if (s.Has(ColorKey))
        {
            Color = s.Get(ColorKey);
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);

        if (Color is not null)
        {
            s.Set(ColorKey, Color.Value);
        }
    }

    public void DeleteEntity()
    {
        var material = Renderer?.material;
        if (material is not null)
        {
            Destroy(material);
        }
    }
}
