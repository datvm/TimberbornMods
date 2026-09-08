global using Timberborn.BlockSystem;
global using Timberborn.EntitySystem;
using Timberborn.Coordinates;

namespace ColorfulBuildings;

public class BuildingColorComponent : BaseComponent, IPersistentEntity, IDeletableEntity
{
    static readonly ComponentKey SaveKey = new("BuildingColor");
    static readonly PropertyKey<Vector3Int> ColorKey = new("Color");
    static readonly PropertyKey<int> RotationKey = new("Rotation");

    public Vector3Int? Color { get; private set; }
    Color? originalColor;
    public int? Rotation { get; private set; }

    Transform _transform = null!;
    BlockObject? blockObject;
    float currentRotation = 0f;

    public Renderer? Renderer { get; private set; }

    public void SetColor(Vector3Int color)
    {
        var material = Renderer?.material;
        if (!material) { return; }

        Color = color;
        var c = new Color(color.x / 255f, color.y / 255f, color.z / 255f);
        SetColorToMaterial(material, c);
    }

    public void SetRotation(int rotation)
    {
        if (!Renderer || !blockObject) { return; }

        var actualRotation = rotation - currentRotation;
        currentRotation = rotation;
        Rotation = rotation;

        var size = CoordinateSystem.GridToWorld(blockObject.BlocksSpec.Size);
        _transform.Translate(size / 2f);
        _transform.Rotate(Vector3.up, actualRotation, Space.Self);
        _transform.Translate(-size / 2f);
    }

    public void ClearColor()
    {
        Color = null;

        var material = Renderer?.material;
        if (material is null) { return; }

        ClearColorToMaterial(material);
    }

    public void Reset()
    {
        ClearColor();
        Rotation = null;
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
        _transform = TransformFast;
        blockObject = GetComponentFast<BlockObject>();
    }

    public void Start()
    {
        if (Color is not null)
        {
            SetColor(Color.Value);
        }

        if (Rotation is not null)
        {
            SetRotation(Rotation.Value);
        }
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(ColorKey))
        {
            Color = s.Get(ColorKey);
        }

        if (s.Has(RotationKey))
        {
            Rotation = s.Get(RotationKey);
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);

        if (Color is not null)
        {
            s.Set(ColorKey, Color.Value);
        }

        if (Rotation is not null)
        {
            s.Set(RotationKey, Rotation.Value);
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
