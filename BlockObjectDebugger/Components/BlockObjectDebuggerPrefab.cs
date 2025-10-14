namespace BlockObjectDebugger.Components;

public class BlockObjectDebuggerPrefab(BlockOccupations occupation)
{
        
    GameObject CreateBuildingModel(string name)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;

        var t = go.transform;
        t.position = new(.5f, .5f, .5f);
        t.localScale = new(.3f, .3f, .3f);

        return go;
    }

    public GameObject Build()
    {
        var go = new GameObject($"{nameof(BlockObjectDebugger)}_{occupation}");

        var finished = CreateBuildingModel("#Finished");
        var unfinished = CreateBuildingModel("#Unfinished");
        finished.transform.parent = unfinished.transform.parent = go.transform;

        var building = go.AddComponent<BuildingSpec>();
        building._buildingCost = [];
        building._placeFinished = true;

        var buildingModel = go.AddComponent<BuildingModelSpec>();
        buildingModel._finishedModelName = "#Finished";
        buildingModel._unfinishedModelName = "#Unfinished";

        var prefab = go.AddComponent<PrefabSpec>();
        var prefabName = prefab._prefabName = $"{ModUtils.GetPrefabName(occupation.ToString())}";
        prefab._backwardCompatiblePrefabNames = [];

        var bo = go.AddComponent<BlockObjectSpec>();
        bo._blocksSpec = new()
        {
            _size = Vector3Int.one,
            _blockSpecs = [
                new() {
                    _matterBelow = MatterBelow.GroundOrStackable,
                    _occupations = occupation,
                    _stackable = BlockStackable.BlockObject,
                    _useNewOccupation = true,
                }
            ],
        };
        bo._entrance = new();

        var placable = go.AddComponent<PlaceableBlockObjectSpec>();
        placable._toolGroupId = "MapEditor";
        placable._toolOrder = 2000 + (int)occupation;
        placable._layout = BlockObjectLayout.Rectangle;

        var label = go.AddComponent<LabeledEntitySpec>();
        label._imagePath = "Buildings/Landscaping/Terrainblock/TerrainBlockIcon";

        var nameKey = label._displayNameLocKey = $"LV.BOD.{prefabName}";
        ModUtils.AppendLoc(nameKey, "Debug Occupation " + occupation);
        
        var descKey = label._descriptionLocKey = $"LV.BOD.{prefabName}.Desc";
        ModUtils.AppendLoc(descKey, "A debug block object for occupation " + occupation);

        
        var debugger = go.AddComponent<BlockObjectDebuggerComponentSpec>();
        debugger.occupations = occupation;

        return go;
    }

}
