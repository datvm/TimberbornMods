namespace BuildingBlueprints.Services;

[BindSingleton]
public class BuildingBlueprintsService(
    TemplateNameMapper templateNameMapper,
    ToolUnlockingService toolUnlockingService,
    ToolButtonService toolButtonService,
    ScienceService scienceService,
    BuildingUnlockingService buildingUnlockingService,
    BuildingBlueprintListingService listingService,
    BlueprintBuildingSettingsService buildingSettingsService,
    BuildingBlueprintTagService tagService
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

        var bp = new SerializableBuildingBlueprint((sx, sy), [.. ProcessBuildings(selection)])
        {
            Name = name,
        };
        BuildingBlueprintPersistentService.SaveBlueprintToFile(name, bp);

        parsedCache = null;
    }

    IEnumerable<SerializableBuildingPlacement> ProcessBuildings(BlueprintSelectionInfo selection)
    {
        var baseCoord = new Vector3Int(selection.Area.xMin, selection.Area.yMin, selection.BaseZ);

        var copySettings = selection.CopySettings;
        foreach (var bo in selection.BlockObjects)
        {
            var coord = bo.Coordinates - baseCoord;
            var template = bo.GetComponent<TemplateSpec>().TemplateName;

            yield return new(
                template,
                (coord.x, coord.y, coord.z), bo.Orientation, bo.FlipMode.IsFlipped,
                copySettings ? buildingSettingsService.GetSettings(bo) : null,
                bo.GetEntityId());
        }
    }

    public bool FilterSelection(BlockObject bo, RectInt area)
    {
        if (!bo || !bo.HasComponent<BuildingSpec>() || !bo.HasComponent<PlaceableBlockObjectSpec>()) { return false; }

        // Blacklist certain kind of District buildings
        if (bo.HasComponent<DistrictCenterSpec>()
            || bo.HasComponent<LinkedBuildingSpec>())
        {
            return false;
        }

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

        foreach (var bp in listingService.GetBlueprints())
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
                    rawB.OriginalId,
                    new(x, y, z),
                    rawB.Orientation,
                    rawB.Flip ? FlipMode.Flipped : FlipMode.Unflipped,
                    rawB.Settings
                ));
            }

            IEnumerable<string> tags = bp.Tags;
            if (bp.Tags.Count == 0)
            {
                tags = tags.Append(tagService.UntaggedName);
            }

            var (sx, sy) = bp.Size;
            parsedCache.Add(new(
                bp,
                [.. placements],
                [.. counter],
                [.. cost.Select(kv => new GoodAmount(kv.Key, kv.Value))],
                [..tags]
            ));
        }

        tagService.OnBlueprintCacheUpdated(parsedCache);
    }

    ParsedBlueprintBuilding ParseBuilding(SerializableBuildingPlacement building)
    {
        templateNameMapper.TryGetTemplate(building.TemplateName, out var template);

        return template is null
            ? new(building.TemplateName, null, null, null, null)
            : new(
                building.TemplateName,
                template.GetSpec<PlaceableBlockObjectSpec>(),
                template.GetSpec<LabeledEntitySpec>(),
                template.GetSpec<BuildingSpec>(),
                template.GetSpec<BlockObjectSpec>());
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

                if (IsLocked(name, out var tool))
                {
                    v.LockedTools.Add(b);

                    if (tool is BlockObjectTool bot)
                    {
                        var cost = bot.Template.GetSpec<BuildingSpec>().ScienceCost;
                        v.ScienceCost += cost;
                    }
                }
            }
        }

        return result;

        bool IsLocked(string templateName, [NotNullWhen(true)] out ITool? tool)
            => toolByTemplateName.TryGetValue(templateName, out tool)
            && toolUnlockingService.IsLocked(tool);
    }

    public IEnumerable<ParsedValidatedBlueprintTag> GetValidatedBlueprintTags()
    {
        var bps = GetParsedBlueprintsWithValidation()
            .ToDictionary(bp => bp.Blueprint.Name);

        foreach (var tag in tagService.Tags)
        {
            yield return new(
                tag.Name,
                [.. tag.Blueprints.Select(bp => bps[bp.Name])]
            );
        }
    }

    public bool HasEnoughScience(int requirement) => scienceService.SciencePoints >= requirement;

    public void UnlockToolsForBlueprint(BlueprintWithValidation bp)
    {
        if (!bp.HasLockedTools) { return; }

        if (bp.ScienceCost > 0)
        {
            scienceService.SubtractPoints(bp.ScienceCost);
        }

        foreach (var t in bp.LockedTools)
        {
            buildingUnlockingService.UnlockIgnoringCost(t.BuildingSpec);
            toolUnlockingService.Unlock(toolByTemplateName[t.TemplateName]);
        }
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

