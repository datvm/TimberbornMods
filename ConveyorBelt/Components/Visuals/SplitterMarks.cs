namespace ConveyorBelt.Components.Visuals;

public static class SplitterMarks
{
    public static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    public static readonly int ColorId = Shader.PropertyToID("_Color");

    public static readonly Color Left = new(1f, 0.84f, 0.15f, 1f);
    public static readonly Color Center = Color.red;
    public static readonly Color Right = new(1f, 0.48f, 0.08f, 1f);

    public static Color For(BeltDir direction) => direction switch
    {
        BeltDir.Left => Left,
        BeltDir.Right => Right,
        _ => Center,
    };
}
