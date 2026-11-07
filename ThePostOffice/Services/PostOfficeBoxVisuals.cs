
namespace ThePostOffice.Services;

[BindSingleton]
public class PostOfficeBoxVisuals(
    IAssetLoader assets,
    IGoodService goods,
    GoodIconVisualizer icons,
    MaterialHeightCutoffSetter cutoffs,
    GoodVisualizationSpecService visualizations,
    PostOfficeGlobalInventoryService inventory,
    EventBus events
) : ILoadableSingleton
{
    const string IconPath = "Blueprints/BlockObjectToolGroupSpecs/PostOfficeIcon";
    const string BoxVisualization = "Box";
    const string BoxVariant = "8H";

    static readonly int DetailMap = Shader.PropertyToID("_DetailAlbedoMap2");
    static readonly int DetailColor = Shader.PropertyToID("_DetailAlbedoUV2Color");
    static readonly Color BannerColor = new(0.33f, 0.33f, 0.33f);

    readonly MeshBuilder builder = new();

    Sprite? icon;

    public event Action? LodgeFillChanged;

    public void Load()
    {
        events.Register(this);
    }

    [OnEvent]
    public void OnInventoryChanged(OnPostOfficeGlobalInventoryChanged _) => LodgeFillChanged?.Invoke();

    [OnEvent]
    public void OnCapacityChanged(OnPostOfficeCapacityChanged _) => LodgeFillChanged?.Invoke();

    public List<BoxShare> LodgeShares()
    {
        List<BoxAmount> weights = [];
        foreach (var pair in inventory.Inventory)
        {
            var weight = pair.Value.TotalWeight;
            if (weight <= 0)
            {
                continue;
            }

            weights.Add(new(pair.Key, weight));
        }

        return PostOfficeBoxPlan.FromCapacity(
            weights,
            inventory.TotalWeight,
            inventory.Capacity,
            PostOfficeBoxPlan.LodgeSlots.Count);
    }

    public List<BoxShare> OfficeShares(Inventory stock, int capacity)
    {
        List<BoxAmount> weights = [];
        var total = 0;
        foreach (var good in stock.Stock)
        {
            if (good.Amount <= 0)
            {
                continue;
            }

            var spec = goods.GetGoodOrNull(good.GoodId);
            if (spec == null || spec.Weight <= 0)
            {
                continue;
            }

            var weight = spec.Weight * good.Amount;
            total += weight;
            weights.Add(new(good.GoodId, weight));
        }

        return PostOfficeBoxPlan.FromCapacity(weights, total, capacity, PostOfficeBoxPlan.OfficeSlots.Count);
    }

    public void Show(PostOfficeBoxPile pile, Transform parent, IReadOnlyList<BoxShare> shares, IReadOnlyList<BoxSlot> slots, int columnLayers)
    {
        pile.Clear();
        var cursor = 0;
        foreach (var share in shares)
        {
            if (share.Count <= 0)
            {
                continue;
            }

            cursor = ColumnStart(cursor, columnLayers);
            if (cursor >= slots.Count)
            {
                break;
            }

            var count = Math.Min(share.Count, slots.Count - cursor);
            pile.Add(CreateGood(parent, share.Id, slots, cursor, count));
            cursor += count;
        }
    }

    public void ApplyBanner(MeshRenderer renderer, bool flipped, List<Material> created)
    {
        var filter = renderer.GetComponent<MeshFilter>();
        var mesh = filter ? filter.sharedMesh : null;
        if (!mesh)
        {
            return;
        }

        var texture = Icon().texture;
        var shared = renderer.sharedMaterials;
        List<Vector2> uvs = [];
        mesh.GetUVs(1, uvs);
        var changed = false;
        for (var i = 0; i < shared.Length; i++)
        {
            var material = shared[i];
            if (!material || !FlagSubmesh(mesh, uvs, i))
            {
                continue;
            }

            var copy = new Material(material);
            copy.SetTexture(DetailMap, texture);
            copy.SetColor(DetailColor, BannerColor);
            if (flipped)
            {
                copy.SetTextureScale(DetailMap, new(-1f, 1f));
                copy.SetTextureOffset(DetailMap, new(1f, 0f));
            }

            shared[i] = copy;
            created.Add(copy);
            changed = true;
        }

        if (changed)
        {
            renderer.sharedMaterials = shared;
        }
    }

    static bool FlagSubmesh(Mesh mesh, List<Vector2> uvs, int submesh)
    {
        if (uvs.Count == 0 || submesh >= mesh.subMeshCount)
        {
            return false;
        }

        var indices = mesh.GetIndices(submesh);
        if (indices.Length == 0 || indices.Length > 24 * 3)
        {
            return false;
        }

        var min = new Vector2(float.MaxValue, float.MaxValue);
        var max = new Vector2(float.MinValue, float.MinValue);
        foreach (var index in indices)
        {
            if ((uint)index >= (uint)uvs.Count)
            {
                continue;
            }

            var uv = uvs[index];
            min = Vector2.Min(min, uv);
            max = Vector2.Max(max, uv);
        }

        return max.x - min.x > 0.9f && max.y - min.y > 0.9f;
    }

    GameObject CreateGood(Transform parent, string id, IReadOnlyList<BoxSlot> slots, int start, int count)
    {
        var spec = visualizations.GetVisualization(BoxVisualization, BoxVariant);
        var step = spec.Offset.z;
        var limit = Math.Max(1, spec.LimitingAmountFlooredToInt);
        var floor = slots[start].Y;
        List<Column> columns = [];
        var index = 0;
        var tallest = 0;
        while (index < count)
        {
            var column = slots[start + index];
            var height = 0;
            while (index < count)
            {
                var slot = slots[start + index];
                if (slot.X != column.X || slot.Z != column.Z)
                {
                    break;
                }

                height++;
                index++;
            }

            columns.Add(new(column.X, column.Y, column.Z, height));
            if (height > tallest)
            {
                tallest = height;
            }
        }

        var body = new GameObject("PostOfficeBoxes");
        body.transform.SetParent(parent, worldPositionStays: false);
        var good = goods.GetGood(id);
        for (var band = 0; band * limit < tallest; band++)
        {
            var bandBase = band * limit;
            builder.Reset("PostOfficeBoxes");
            var any = false;
            foreach (var column in columns)
            {
                var shown = Math.Min(limit, column.Height - bandBase);
                if (shown <= 0)
                {
                    continue;
                }

                any = true;
                var top = column.Y + (bandBase + shown) * step;
                builder.AppendMesh(
                    spec.PrimaryMesh.Asset,
                    [spec.Material.Asset],
                    new TranslationTransform(new(column.X, top, column.Z)));
            }

            if (!any)
            {
                continue;
            }

            var built = builder.Build();
            var piece = new GameObject("PostOfficeBoxes");
            piece.transform.SetParent(body.transform, worldPositionStays: false);
            piece.AddComponent<MeshFilter>().sharedMesh = built.Mesh;
            var material = new Material(spec.Material.Asset);
            icons.ShowIcon(material, good);
            var cutoff = floor + bandBase * step;
            cutoffs.SetCutoff(material, parent.TransformPoint(new(0f, cutoff, 0f)).y - 0.01f);
            piece.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        return body;
    }

    static int ColumnStart(int index, int layers)
    {
        if (layers <= 1)
        {
            return index;
        }

        var into = index % layers;
        if (into == 0)
        {
            return index;
        }

        return index + layers - into;
    }

    Sprite Icon() => icon ??= assets.Load<Sprite>(IconPath);
}

public sealed class PostOfficeBoxPile
{
    readonly List<GameObject> objects = [];

    public void Add(GameObject created) => objects.Add(created);

    public void Clear()
    {
        foreach (var body in objects)
        {
            if (!body)
            {
                continue;
            }

            foreach (var filter in body.GetComponentsInChildren<MeshFilter>(includeInactive: true))
            {
                if (filter.sharedMesh)
                {
                    Object.Destroy(filter.sharedMesh);
                }
            }

            foreach (var renderer in body.GetComponentsInChildren<MeshRenderer>(includeInactive: true))
            {
                if (renderer.sharedMaterial)
                {
                    Object.Destroy(renderer.sharedMaterial);
                }
            }

            Object.Destroy(body);
        }

        objects.Clear();
    }
}

readonly record struct Column(float X, float Y, float Z, int Height);
