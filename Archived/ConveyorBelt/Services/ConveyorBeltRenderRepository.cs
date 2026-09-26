namespace ConveyorBelt.Services;

[BindSingleton]
public class ConveyorBeltRenderRepository : ILoadableSingleton, IUnloadableSingleton
{
    const float ArrowLength = 0.72f;
    const float ArrowShaftWidth = 0.08f;
    const float ArrowHeadWidth = 0.18f;
    const float ArrowHeadLength = 0.22f;

    static readonly Color ArrowColor = Color.red;

    public Material ArrowMaterial { get; private set; } = null!;
    public UnityEngine.Mesh ArrowMesh { get; private set; } = null!;

    public bool ArrowVisible { get; private set; }
    public event EventHandler<bool>? OnArrowVisibilityRequested;

    public void Load()
    {
        var shader = Shader.Find("Universal Render Pipeline/Unlit")
            ?? Shader.Find("Unlit/Color")
            ?? Shader.Find("Sprites/Default");

        ArrowMaterial = new(shader)
        {
            color = ArrowColor,
        };

        ArrowMesh = CreateArrowMesh();
    }

    public void Unload()
    {
        Object.Destroy(ArrowMaterial);
        Object.Destroy(ArrowMesh);
    }

    public void RequestArrowVisibility(bool visible)
    {
        ArrowVisible = visible;
        OnArrowVisibilityRequested?.Invoke(this, visible);
    }

    static UnityEngine.Mesh CreateArrowMesh()
    {
        var halfShaftWidth = ArrowShaftWidth / 2f;
        var halfHeadWidth = ArrowHeadWidth / 2f;
        var tail = Vector3.down * (ArrowLength / 2f);
        var tip = Vector3.up * (ArrowLength / 2f);
        var headBase = tip - Vector3.up * ArrowHeadLength;

        var mesh = new UnityEngine.Mesh
        {
            name = "ConveyorBeltDirectionArrow",
            vertices =
            [
                tail - Vector3.right * halfShaftWidth,
                tail + Vector3.right * halfShaftWidth,
                headBase + Vector3.right * halfShaftWidth,
                headBase + Vector3.right * halfHeadWidth,
                tip,
                headBase - Vector3.right * halfHeadWidth,
                headBase - Vector3.right * halfShaftWidth,
            ],
            triangles =
            [
                0, 1, 2,
                0, 2, 6,
                6, 2, 3,
                6, 3, 5,
                5, 3, 4,
                2, 1, 0,
                6, 2, 0,
                3, 2, 6,
                5, 3, 6,
                4, 3, 5,
            ],
        };

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }
}
