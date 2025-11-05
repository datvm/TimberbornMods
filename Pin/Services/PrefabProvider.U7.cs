namespace Pin.Services;

public class PrefabAssetProvider : IAssetProvider
{
    public const string PinPrefabPath = "Resources/Buildings/Decoration/Pin/";
    public const string FolktailPrefab = "Pin.Folktails";
    public const string IronTeethPrefab = "Pin.IronTeeth";
    public const string FolktailPrefabPath = PinPrefabPath + FolktailPrefab;
    public const string IronTeethPrefabPath = PinPrefabPath + IronTeethPrefab;
    static readonly FrozenSet<string> PrefabPaths = FrozenSet.Create(StringComparer.OrdinalIgnoreCase, [FolktailPrefabPath, IronTeethPrefabPath]);

    public bool IsBuiltIn { get; }

    public IEnumerable<OrderedAsset> LoadAll<T>(string path) where T : UnityEngine.Object => [];

    public void Reset()
    {
    }

    public bool TryLoad(string path, Type type, out OrderedAsset orderedAsset)
    {
        if (type != typeof(GameObject) || !PrefabPaths.Contains(path))
        {
            orderedAsset = default!;
            return false;
        }

        var isFt = path.EndsWith(FolktailPrefab, StringComparison.OrdinalIgnoreCase);
        orderedAsset = new OrderedAsset(0, CreatePrefab(isFt));
        return true;
    }

    static GameObject CreatePrefab(bool ft)
    {
        var parent = new GameObject();
        parent.SetActive(false);

        var prefabName = ft ? FolktailPrefab : IronTeethPrefab;
        var prefab = new GameObject(prefabName);
        prefab.transform.parent = parent.transform;

        var building = prefab.AddComponent<BuildingSpec>();
        building._loopingSoundName = "";
        building._buildingCost = [];
        building._placeFinished = true;
        building._finishableWithBeaversOnSite = true;

        var buildingModel = prefab.AddComponent<BuildingModelSpec>();
        buildingModel._finishedModelName = "#Finished";
        buildingModel._unfinishedModelName = "#Unfinished";
        buildingModel._constructionModeModel = BuildingModelSpec.ConstructionModeModel.Finished;

        var prefabSpec = prefab.AddComponent<PrefabSpec>();
        prefabSpec._prefabName = prefabName;
        prefabSpec._backwardCompatiblePrefabNames = [];

        var blockObj = prefab.AddComponent<BlockObjectSpec>();
        blockObj._blocksSpec = new()
        {
            _size = new(1, 1, 1),
            _blockSpecs = [
                new() {
                    _matterBelow = MatterBelow.GroundOrStackable,
                    _occupations = BlockOccupations.Path,
                    _stackable = BlockStackable.None,
                    _useNewOccupation = true,
                }
            ],
        };
        blockObj._entrance = new();

        var placable = prefab.AddComponent<PlaceableBlockObjectSpec>();
        placable._toolGroupId = "Decoration";
        placable._toolOrder = -10;
        placable._toolShape = ToolShapes.Square;
        placable._layout = BlockObjectLayout.Single;
        placable._customPivot = new();

        var label = prefab.AddComponent<LabeledEntitySpec>();
        label._displayNameLocKey = "LV.Pin.PinName";
        label._descriptionLocKey = "LV.Pin.PinDesc";
        label._flavorDescriptionLocKey = "LV.Pin.PinFlavor";
        label._imagePath = "Resources/Buildings/Decorations/PinIcon";

        prefab.AddComponent<PinSpec>();

        AttachChildren(prefab.transform, ft);

        return prefab;
    }

    static void AttachChildren(Transform prefabTransform, bool ft)
    {
        var unfinished = new GameObject("#Unfinished");
        unfinished.transform.parent = prefabTransform;

        var finished = new GameObject("#Finished");
        finished.transform.parent = prefabTransform;

        var mesh = finished.AddComponent<TimbermeshDescription>();
        mesh._modelName = $@"Buildings/Paths/Path/DirtPath0000.{(ft ? "Folktails" : "IronTeeth")}.Model";

        var collider = finished.AddComponent<BoxCollider>();
        collider.center = new(.5f, .02f, .5f);
        collider.size = new(1f, .04f, 1f);

        var cyclinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cyclinder.name = "#Pole";
        var t = cyclinder.transform;
        t.parent = prefabTransform;
        t.localPosition = new(.5f, 1.5f, .5f);
        t.localScale = new(.1f, 1.5f, .1f);
    }

}
