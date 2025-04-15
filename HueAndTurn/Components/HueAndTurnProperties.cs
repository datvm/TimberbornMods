namespace HueAndTurn.Components;

public record HueAndTurnProperties
{
    static readonly PropertyKey<Color> ColorKey = new(nameof(Color));
    static readonly PropertyKey<int> RotationKey = new(nameof(Rotation));
    static readonly PropertyKey<Vector2Int> RotationPivotKey = new(nameof(RotationPivot));
    static readonly PropertyKey<Vector3Int> TranslationKey = new(nameof(Translation));

    public Color? Color { get; set; }
    public int? Rotation { get; set; }
    public Vector2Int? RotationPivot { get; set; }
    public Vector3Int? Translation { get; set; }

    public bool IsDefault =>
        Color is null
        && Rotation is null
        && RotationPivot is null
        && Translation is null;

    public void Save(IObjectSaver s)
    {
        if (Color is not null)
        {
            s.Set(ColorKey, Color.Value);
        }

        if (Rotation is not null)
        {
            s.Set(RotationKey, Rotation.Value);
        }

        if (RotationPivot is not null)
        {
            s.Set(RotationPivotKey, RotationPivot.Value);
        }

        if (Translation is not null)
        {
            s.Set(TranslationKey, Translation.Value);
        }
    }

    public static HueAndTurnProperties Load(IObjectLoader s, IObjectLoader? legacy)
    {
        try
        {
            HueAndTurnProperties props = new()
            {
                Color = s.Has(ColorKey) ? s.Get(ColorKey) : null,
                Rotation = s.Has(RotationKey) ? s.Get(RotationKey) : null,
                RotationPivot = s.Has(RotationPivotKey) ? s.Get(RotationPivotKey) : null,
                Translation = s.Has(TranslationKey) ? s.Get(TranslationKey) : null,
            };

            if (legacy is not null)
            {
                if (legacy.Has(RotationPivotKey))
                {
                    props.RotationPivot = legacy.Get(RotationPivotKey);
                }
                if (legacy.Has(TranslationKey))
                {
                    props.Translation = legacy.Get(TranslationKey);
                }
            }

            return props;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            return new();
        }        
    }

}