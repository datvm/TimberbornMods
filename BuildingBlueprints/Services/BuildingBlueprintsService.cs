namespace BuildingBlueprints.Services;

[BindSingleton]
public class BuildingBlueprintsService(
    TemplateNameMapper templateMapper,
    ToolUnlockingService toolUnlockingService,
    ToolButtonService toolButtonService
) : IPostLoadableSingleton
{

    List<ParsedBlueprintInfo>? parsedCache;
    FrozenDictionary<string, ITool> toolByTemplateName = null!;

    public void PostLoad()
    {
        Dictionary<string, ITool> tools = [];

        foreach (var t in toolButtonService.ToolButtons)
        {
            if (t.Tool is BlockObjectTool bot)
            {
                var name = bot.Template.GetSpec<TemplateSpec>().TemplateName;
                tools[name] = bot;
            }
        }

        toolByTemplateName = tools.ToFrozenDictionary();
    }

    public void ProcessAndSaveBlueprint(string name, BlueprintSelectionInfo selection)
    {
        var (sx, sy) = selection.Area.size;

        var bp = new SerializableBuildingBlueprint(name, (sx, sy), [.. ProcessBuildings(selection)]);
        BuildingBlueprintPersistentService.SaveBlueprintToFile(name, bp);

        parsedCache = null;
    }

    IEnumerable<SerializableBuildingPlacement> ProcessBuildings(BlueprintSelectionInfo selection)
    {
        var baseCoord = new Vector3Int(selection.Area.xMin, selection.Area.yMin, selection.BaseZ);

        foreach (var bo in selection.BlockObjects)
        {
            var coord = bo.Coordinates - baseCoord;
            var template = bo.GetComponent<TemplateSpec>().TemplateName;

            yield return new(template, (coord.x, coord.y, coord.z), bo.Orientation, bo.FlipMode.IsFlipped);
        }
    }

    public bool FilterSelection(BlockObject bo, RectInt area)
    {
        if (!bo || !bo.HasComponent<BuildingSpec>() || !bo.HasComponent<PlaceableBlockObjectSpec>()) { return false; }

        foreach (var pos in bo.PositionedBlocks.GetAllBlocks())
        {
            if (!area.Contains(pos.Coordinates.XY()))
            {
                return false;
            }
        }

        return true;
    }

    void ParseBlueprints()
    {
        parsedCache = [];
        Dictionary<string, ParsedBlueprintBuilding> buildingsCache = [];

        foreach (var bp in BuildingBlueprintPersistentService.GetBlueprints())
        {

            Dictionary<ParsedBlueprintBuilding, int> counter = [];
            Dictionary<string, int> cost = [];
            List<ParsedBlueprintBuildingPlacement> placements = [];

            foreach (var rawB in bp.Buildings)
            {
                var b = buildingsCache.GetOrAdd(rawB.TemplateName, () => ParseBuilding(rawB));

                counter[b] = counter.GetValueOrDefault(b) + 1;
                if (!b.Missing)
                {
                    foreach (var g in b.BuildingSpec!.BuildingCost)
                    {
                        cost[g.Id] = cost.GetValueOrDefault(g.Id) + g.Amount;
                    }
                }

                var (x, y, z) = rawB.Coordinates;
                placements.Add(new(
                    b,
                    new(x, y, z),
                    rawB.Orientation,
                    rawB.Flip ? FlipMode.Flipped : FlipMode.Unflipped
                ));
            }

            var (sx, sy) = bp.Size;
            parsedCache.Add(new(
                bp.Name, new(sx, sy),
                [.. placements],
                [.. counter],
                [.. cost.Select(kv => new GoodAmount(kv.Key, kv.Value))]
            ));
        }
    }

    ParsedBlueprintBuilding ParseBuilding(SerializableBuildingPlacement building)
    {
        templateMapper.TryGetTemplate(building.TemplateName, out var template);

        return new(
            building.TemplateName,
            template?.GetSpec<PlaceableBlockObjectSpec>(),
            template?.GetSpec<LabeledEntitySpec>(),
            template?.GetSpec<BuildingSpec>()
        );
    }

    public IReadOnlyList<ParsedBlueprintInfo> GetParsedBlueprints(bool forceReload = false)
    {
        if (parsedCache is null || forceReload)
        {
            ParseBlueprints();
        }

        return parsedCache!;
    }

    public IReadOnlyList<BlueprintWithValidation> GetParsedBlueprintsWithValidation(bool forceReload = false)
    {
        List<BlueprintWithValidation> result = [];

        foreach (var bp in GetParsedBlueprints(forceReload))
        {
            var v = new BlueprintWithValidation(bp);
            result.Add(v);

            foreach (var (b, _) in bp.BuildingsCount)
            {
                var name = b.TemplateName;
                if (b.Missing)
                {
                    v.MissingTemplates.Add(name);
                }

                if (IsLocked(name))
                {
                    v.LockedTools.Add(b);
                }
            }
        }

        return result;

        bool IsLocked(string templateName)
            => toolByTemplateName.TryGetValue(templateName, out var tool)
            && toolUnlockingService.IsLocked(tool);
    }

    public static RectInt FromAreaSelection(Vector3Int start, Vector3Int end)
    {
        var x1 = Math.Min(start.x, end.x);
        var x2 = Math.Max(start.x, end.x);
        var y1 = Math.Min(start.y, end.y);
        var y2 = Math.Max(start.y, end.y);

        return new(x1, y1, x2 - x1 + 1, y2 - y1 + 1);
    }

}

