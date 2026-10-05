namespace ConveyorBelt.Services;

[BindSingleton]
public class BeltBoxVisuals(IAssetLoader assets, IGoodService goods, IMaterialRepository materials, FactionService factions)
    : ILoadableSingleton, IUnloadableSingleton
{
    const string MeshPath = "StockpileGoodModels/9BoxColumn";
    const string MaterialPath = "Materials/Goods/Box";
    const float BoxesInColumn = 9f;
    const float SideUv = 1.5f;
    const float IconFill = 0.62f;
    const float IconLift = 0.008f;

    static readonly int ColorId = Shader.PropertyToID("_Color");
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
    static readonly Color DefaultBoxColor = new(152/255f, 120/255f, 81/255f);

    UnityEngine.Mesh? box;
    UnityEngine.Mesh? iconMesh;
    Material? source;
    Shader? iconShader;

    public bool Ready => box && iconMesh;

    public UnityEngine.Mesh Box => box!;

    public UnityEngine.Mesh IconMesh => iconMesh!;

    public float Size { get; private set; }

    public float Height { get; private set; }

    public Material? RunnerWood { get; private set; }

    public void Load()
    {
        var column = assets.Load<UnityEngine.Mesh>(MeshPath);
        source = assets.Load<Material>(MaterialPath);
        box = SingleBox(column);
        Size = box.bounds.size.x;
        Height = box.bounds.size.y;
        iconMesh = IconFaces(Size, Height);
        var wood = factions.Current.Id == "IronTeeth" ? "BaseWood_Grey.IronTeeth" : "BaseWood_LightBrown.Folktails";
        RunnerWood = materials.GetMaterial(wood);
        iconShader = Shader.Find("Sprites/Default")
            ?? Shader.Find("Universal Render Pipeline/Unlit")
            ?? Shader.Find("Unlit/Texture");
    }

    public void Unload()
    {
        if (box)
        {
            Object.Destroy(box);
            box = null;
        }

        if (iconMesh)
        {
            Object.Destroy(iconMesh);
            iconMesh = null;
        }
    }

    public Material CreateMaterial() => new(source);

    public Material CreateIconMaterial() => new(iconShader);

    public bool ApplyGood(Material boxMaterial, Material iconMaterial, string id)
    {
        var spec = goods.GetGoodOrNull(id);
        if (spec is null)
        {
            boxMaterial.SetColor(ColorId, DefaultBoxColor);
            boxMaterial.SetColor(BaseColorId, DefaultBoxColor);
            return false;
        }

        var color = spec.ContainerColor.a > 0f ? spec.ContainerColor : DefaultBoxColor;
        boxMaterial.SetColor(ColorId, color);
        boxMaterial.SetColor(BaseColorId, color);

        var sprite = spec.Icon.Asset;
        if (!sprite || !sprite.texture)
        {
            return false;
        }

        var texture = sprite.texture;
        var rect = sprite.textureRect;
        var scale = new Vector2(rect.width / texture.width, rect.height / texture.height);
        var offset = new Vector2(rect.x / texture.width, rect.y / texture.height);
        SetIcon(iconMaterial, MainTexId, texture, scale, offset);
        SetIcon(iconMaterial, BaseMapId, texture, scale, offset);
        return true;
    }

    static void SetIcon(Material material, int property, Texture texture, Vector2 scale, Vector2 offset)
    {
        material.SetTexture(property, texture);
        material.SetTextureScale(property, scale);
        material.SetTextureOffset(property, offset);
    }

    static UnityEngine.Mesh SingleBox(UnityEngine.Mesh column)
    {
        var mesh = Object.Instantiate(column);
        mesh.name = "BeltBox";
        var positions = mesh.vertices;
        var uv0 = mesh.uv;
        var uv1 = mesh.uv2;
        var hasUv1 = uv1.Length == uv0.Length;

        var minY = float.PositiveInfinity;
        foreach (var position in positions)
        {
            minY = Mathf.Min(minY, position.y);
        }

        var sideMin = float.PositiveInfinity;
        var sideMax = float.NegativeInfinity;
        for (var i = 0; i < uv0.Length; i++)
        {
            var v = uv0[i].y;
            if (Mathf.Abs(v) <= SideUv)
            {
                continue;
            }

            sideMin = Mathf.Min(sideMin, v);
            sideMax = Mathf.Max(sideMax, v);
        }

        var span = sideMax - sideMin;
        for (var i = 0; i < positions.Length; i++)
        {
            positions[i].y = (positions[i].y - minY) / BoxesInColumn;
            if (Mathf.Abs(uv0[i].y) <= SideUv || span <= 0f)
            {
                continue;
            }

            var v = (uv0[i].y - sideMin) / span;
            uv0[i].y = v;
            if (hasUv1)
            {
                uv1[i].y = v;
            }
        }

        mesh.vertices = positions;
        mesh.uv = uv0;
        if (hasUv1)
        {
            mesh.uv2 = uv1;
        }

        mesh.SetUVs(2, uv0);

        mesh.RecalculateBounds();
        return mesh;
    }

    static UnityEngine.Mesh IconFaces(float size, float height)
    {
        var half = size * 0.5f;
        var span = Mathf.Min(size, height) * IconFill;
        var h = span * 0.5f;
        var mid = height * 0.5f;
        var verts = new List<Vector3>();
        var uv = new List<Vector2>();
        var tris = new List<int>();

        void Quad(Vector3 center, Vector3 axis, Vector3 up)
        {
            var start = verts.Count;
            verts.Add(center - axis * h - up * h);
            verts.Add(center + axis * h - up * h);
            verts.Add(center + axis * h + up * h);
            verts.Add(center - axis * h + up * h);
            uv.Add(new(0f, 0f));
            uv.Add(new(1f, 0f));
            uv.Add(new(1f, 1f));
            uv.Add(new(0f, 1f));
            tris.Add(start);
            tris.Add(start + 2);
            tris.Add(start + 1);
            tris.Add(start);
            tris.Add(start + 3);
            tris.Add(start + 2);
            tris.Add(start);
            tris.Add(start + 1);
            tris.Add(start + 2);
            tris.Add(start);
            tris.Add(start + 2);
            tris.Add(start + 3);
        }

        Quad(new(0f, height + IconLift, 0f), Vector3.right, Vector3.forward);
        Quad(new(half + IconLift, mid, 0f), Vector3.forward, Vector3.up);
        Quad(new(-half - IconLift, mid, 0f), Vector3.back, Vector3.up);
        Quad(new(0f, mid, half + IconLift), Vector3.left, Vector3.up);
        Quad(new(0f, mid, -half - IconLift), Vector3.right, Vector3.up);

        var mesh = new UnityEngine.Mesh { name = "BeltBoxIcon" };
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uv);
        mesh.SetTriangles(tris, 0);
        mesh.bounds = new(new(0f, mid, 0f), new(size + 0.05f, height + 0.05f, size + 0.05f));
        return mesh;
    }
}
