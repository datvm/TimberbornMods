namespace Packager.Services;

public class PackagerPrefabProvider : IAssetProvider
{
    public const string ExpectedPrefabName = "LumberjackFlag";
    public const string PrefabPath = "Buildings/Storage/";
    public const string PackagerName = "Packager";
    public const string UnpackagerName = "Unpackager";

    static readonly Material PackagerSignMat;
    static readonly Material UnpackagerSignMat;

    static PackagerPrefabProvider()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        PackagerSignMat = new Material(shader)
        {
            color = new(0, .5f, 0),
        };
        UnpackagerSignMat = new Material(shader)
        {
            color = new(1f, .5f, 0),
        };
    }

    public bool IsBuiltIn { get; } = false;

    public static WeakReference<GameObject>? LumberjackPrefab { get; set; }

    public IEnumerable<OrderedAsset> LoadAll<T>(string path) where T : Object => [];

    public void Reset() { }

    public bool TryLoad(string path, Type type, out OrderedAsset orderedAsset)
    {
        if (type == typeof(GameObject))
        {
            return TryLoadPrefab(path, out orderedAsset);
        }

        orderedAsset = default;
        return false;
    }

    bool TryLoadPrefab(string path, out OrderedAsset orderedAsset)
    {
        orderedAsset = default;
        if (!path.StartsWith(PrefabPath, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var name = Path.GetFileName(path);
        var isPackager = PackagerName.Equals(name, StringComparison.OrdinalIgnoreCase);
        if (!isPackager && !UnpackagerName.Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        orderedAsset = new(0, MakePrefab(isPackager, name));
        return true;
    }

    static GameObject MakePrefab(bool isPackager, string prefabName)
    {
        if (LumberjackPrefab is null || !LumberjackPrefab.TryGetTarget(out var bp))
        {
            throw new InvalidOperationException("The Lumberjack Prefab is not loaded yet.");
        }

        var obj = Object.Instantiate(bp);

        var building = obj.GetComponent<BuildingSpec>();
        building._selectionSoundName = "LumberMill";
        building._scienceCost = 150;
        if (isPackager)
        {
            building._buildingCost = [
                new() { _goodId = "Log", _amount = 10, }
            ];
        }
        else
        {
            building._buildingCost = [
                new() { _goodId = string.Format(PackagedGoodProvider.PackageGoodId, "Log"), _amount = 1, }
            ];
        }

        var workplace = obj.GetComponent<WorkplaceSpec>();
        workplace._workerTypeUnlockCosts = [
            new(){ _scienceCost = 750, _workerType = "Bot", }
        ];

        var prefab = obj.GetComponent<PrefabSpec>();
        prefab._prefabName = prefabName;

        var placeable = obj.GetComponent<PlaceableBlockObjectSpec>();
        placeable._toolGroupId = "Storage";
        placeable._toolOrder = 200 + (isPackager ? 0 : 10);

        var label = obj.GetComponent<LabeledEntitySpec>();
        var key = isPackager ? "LV.Pkg.Packager" : "LV.Pkg.Unpacker";
        label._displayNameLocKey = key;
        label._descriptionLocKey = key + "Desc";
        label._flavorDescriptionLocKey = key + "Flavor";
        label._imagePath = "Resources/UI/Buildings/Storage/" + (isPackager ? "Packager" : "Unpackager");

        var manufactory = obj.AddComponent<PackagerManufactorySpec>();
        manufactory.isPackager = isPackager;

        obj.AddComponent<SimpleManufactoryBehaviorsSpec>();
        obj.AddComponent<WorkshopSpec>();

        DestroyComponent<LumberjackFlagSpec>(obj);
        DestroyComponent<SimpleOutputInventorySpec>(obj);
        DestroyComponent<YieldRemovingBuildingSpec>(obj);

        AttachSign(obj.transform, isPackager);
        return obj;
    }

    static void AttachSign(Transform parent, bool isPackager)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Sign";

        var t = cube.transform;
        t.parent = parent;
        t.localPosition = new(.5f, 1.44f, .5f);
        t.localScale = new(.5f, .5f, .1f);

        var r = cube.GetComponent<Renderer>();
        r.material = isPackager ? PackagerSignMat : UnpackagerSignMat;

        DestroyComponent<Collider>(cube);
    }

    static void DestroyComponent<T>(GameObject obj) where T : Component
    {
        var comp = obj.GetComponent<T>();
        if (!comp)
        {
            Debug.LogWarning($"Component {typeof(T).Name} not found on {obj.name}");
        }

        Object.DestroyImmediate(comp);
    }

}
