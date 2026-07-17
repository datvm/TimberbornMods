namespace PowerLines.UI;

[BindSingleton]
public class PowerLineConnectorRenderer(
    MeshDrawerFactory meshDrawerFactory
) : ILoadableSingleton, IUnloadableSingleton
{
    static readonly Color ConnectorColor = new(1f, 106f / 255, 0);
    
    const float ConnectorRadius = 0.15f;
    static readonly Vector3 Scale = Vector3.one * (ConnectorRadius * 2f);

    public MeshDrawer Drawer { get; private set; } = null!;
    

    Material? material;
    GameObject? meshSource;

    public void Load()
    {
        material = CreateMaterial(ConnectorColor);

        meshSource = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        meshSource.name = "PowerLineConnectorMeshSource";
        meshSource.SetActive(false);
        Object.DontDestroyOnLoad(meshSource);

        // Drop the collider; we only need the mesh
        if (meshSource.TryGetComponent<Collider>(out var collider))
        {
            Object.Destroy(collider);
        }

        var mesh = meshSource.GetComponent<MeshFilter>().sharedMesh;
        Drawer = meshDrawerFactory.Create(mesh, material);
    }

    public void Unload()
    {
        if (meshSource)
        {
            Object.Destroy(meshSource);
            meshSource = null;
        }

        if (material)
        {
            Object.Destroy(material);
            material = null;
        }
    }

    public Matrix4x4 MatrixAtGridPosition(Vector3 gridPosition)
        => Matrix4x4.TRS(
            CoordinateSystem.GridToWorld(gridPosition),
            Quaternion.identity,
            Scale);

    static Material CreateMaterial(Color color)
    {
        var shader = Shader.Find("Universal Render Pipeline/Unlit")
            ?? Shader.Find("Unlit/Color")
            ?? Shader.Find("Sprites/Default");

        return new Material(shader)
        {
            color = color,
            name = $"PowerLineConnectorMaterial-{color}",
        };
    }
}
