global using Timberborn.Beavers;
global using Timberborn.Characters;
global using Timberborn.EntitySystem;

namespace ColorfulBeavers.Graphical;

public class BeaverColorComponent : BaseComponent, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new("BeaverColor");
    static readonly PropertyKey<Vector3Int> ColorKey = new("Color");

    public Vector3Int? Color { get; private set; }

    Character character = null!;
    bool isBeaver;
    CharacterTint? tint;

    public void SetColor(Vector3Int? color)
    {
        if (tint is null) { return; }

        Color = color;
        ResetTint();
    }

    public void ResetTint()
    {
        if (tint is null) { return; }

        if (Color is null)
        {
            tint.DisableTint();
        }
        else
        {
            var cV = Color.Value;
            var c = new Color(cV.x / 255f, cV.y / 255f, cV.z / 255f);
            tint.SetTint(c);
        }
    }

    public void ResetColor()
    {
        Color = null;
        AssignColor();
        SetColor(Color);
    }

    void AssignColor()
    {
        if (Color is not null || // Already assigned
            (!MSettings.ApplyToBot && !isBeaver) || // Not a beaver and not applying to bots
            (!MSettings.AssignRandom && !MSettings.AssignNamed)) // No auto-assign
        {
            return;
        }

        var name = MSettings.AssignNamed ? character.FirstName : null;

        Color = BeaverColorSettings.Instance.AssignColor(name, MSettings.AssignRandom);
    }

    public void Awake()
    {
        character = GetComponentFast<Character>();
        isBeaver = GetComponentFast<BeaverSpec>() is not null;
        tint = GetComponentFast<CharacterTint>();
    }

    public void Start()
    {
        AssignColor();
        SetColor(Color);
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

}
