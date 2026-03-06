namespace TimberLive.Services;

[SelfService(Lifetime = ServiceLifetime.Singleton)]
public class TemplateBlueprintExportService(Loc t, BlueprintApiService blueprintApi)
{

    readonly List<(string, string)> outputLines = [];
    Dictionary<string, ParsedNeedSpec> needsByIds = [];
    Dictionary<string, ParsedNeedGroupSpec> needGroupsByIds = [];

    async Task InitializeAsync()
    {
        needsByIds = (await blueprintApi.GetSpecsAsync<ParsedNeedSpec>())
            .ToDictionary(n => n.Id);
        needGroupsByIds = (await blueprintApi.GetSpecsAsync<ParsedNeedGroupSpec>())
            .ToDictionary(n => n.Id);
    }

    void CleanUp()
    {
        outputLines.Clear();
        needsByIds = [];
    }

    public async Task<Stream> ExportWikiBuildings(FactionTemplateCompilation compilation)
    {
        await InitializeAsync();

        Dictionary<string, List<(ParsedFactionSpec Faction, ParsedBuildingBlueprint Building)>> buildingsGroupped = [];
        var toolGroupsByIds = compilation.ToolGroups.ToDictionary(tg => tg.Id);

        var factionCount = compilation.Factions.Length;

        foreach (var (f, bps) in compilation.FactionsWithTemplates.Values)
        {
            foreach (var b in bps.Buildings)
            {
                var nameLoc = b.DisplayNameLoc;
                buildingsGroupped.GetOrAdd(nameLoc, _ => []).Add((f, b));
            }
        }

        var memoryStream = new MemoryStream();
        using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var buildingGrp in buildingsGroupped.Values)
            {
                try
                {
                    if (buildingGrp.Count != factionCount)
                    {
                        outputLines.Add(("faction", string.Join(" | ", buildingGrp.Select(grp => grp.Faction.DisplayName))));
                    }

                    var b = buildingGrp[0].Building;
                    var name = ParseLabel(b);
                    ParseBlockObject(b);
                    ParseCommonBuilding(b, toolGroupsByIds);
                    ParsePower(b);
                    ParseWaterObject(b);
                    ParseAttraction(b);
                    ParseGoodConsuming(b);

                    var output = string.Join("\n", outputLines.Select(l => $"|{l.Item1}={l.Item2}"));
                    outputLines.Clear();

                    var fileName = $"{name.Replace(' ', '_')}.txt";
                    var entry = zip.CreateEntry(fileName);
                    using var entryStream = entry.Open();
                    using var writer = new StreamWriter(entryStream);
                    writer.Write(output);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error while exporting " + buildingGrp.First().Building.TemplateName, ex);
                }
            }
        }

        CleanUp();

        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    string ParseLabel(ParsedBuildingBlueprint b)
    {
        var label = b.LabeledEntitySpec;

        outputLines.Add(("description", t.T(label.DescriptionLocKey)));

        var flavorParts = t.T(label.FlavorDescriptionLocKey).Split('-');
        outputLines.Add(("flavor", flavorParts[0].Trim()));
        if (flavorParts.Length > 1)
        {
            outputLines.Add(("author", flavorParts[1].Trim()));
        }

        return t.T(label.DisplayNameLocKey);
    }

    void ParseWaterObject(ParsedBuildingBlueprint b)
    {
        if (b.Blueprint.HasComponent<ParsedWaterObjectSpec>())
        {
            outputLines.Add(("water-dependent", "yes"));
        }
    }

    void ParseBlockObject(ParsedBuildingBlueprint b)
    {
        var bos = b.Blueprint.GetComponent<ParsedBlockObjectSpec>();
        var size = bos.Size;
        outputLines.Add(("footprint", $"{size.X}x{size.Y}"));
        outputLines.Add(("height", size.Z.ToString()));

        var solid = false;
        var groundOnly = false;
        var aboveGround = false;

        foreach (var block in bos.Blocks)
        {
            if (block.Stackable != ParsedBlockStackable.None)
            {
                solid = true;
            }

            if (block.MatterBelow == ParsedMatterBelow.Ground)
            {
                groundOnly = true;
            }
            else if (block.MatterBelow == ParsedMatterBelow.Stackable)
            {
                aboveGround = true;
            }
        }

        if (solid)
        {
            outputLines.Add(("solid", "yes"));
        }

        if (groundOnly)
        {
            outputLines.Add(("ground-only", "yes"));
        }

        if (aboveGround)
        {
            outputLines.Add(("above-ground", "yes"));
        }
    }

    void ParseCommonBuilding(ParsedBuildingBlueprint b, Dictionary<string, ParsedBlockObjectToolGroupSpec> toolGroupsByIds)
    {
        var tgName = t.T(toolGroupsByIds[b.PlaceableBlockObjectSpec.ToolGroupId].NameLocKey);
        outputLines.Add(("tool group", $"[[{tgName}]]"));

        var bs = b.Blueprint.GetOptionalComponent<ParsedBuildingSpec>();
        if (bs is not null) // Dev tools etc does not have this
        {
            outputLines.Add(("unlock", bs.ScienceCost.ToString()));

            foreach (var g in bs.BuildingCost)
            {
                outputLines.Add((g.Id, g.Amount.ToString()));
            }
        }
    }

    void ParsePower(ParsedBuildingBlueprint b)
    {
        if (b.Blueprint.HasComponent<ParsedTransputProviderSpec>())
        {
            outputLines.Add(("transmit-power", "yes"));
        }

        var mechNode = b.Blueprint.GetOptionalComponent<ParsedMechanicalNodeSpec>();
        if (mechNode is null) { return; }

    }

    void ParseAttraction(ParsedBuildingBlueprint b)
    {
        var attr = b.Blueprint.GetOptionalComponent<ParsedAttractionSpec>();
        if (attr is null) { return; }

        var needs = attr.Effects.Select(eff =>
        {
            var n = needsByIds[eff.NeedId];
            var group = needGroupsByIds[n.NeedGroupId];

            return (group, n);
        }).ToArray();

        if (needs.Length > 0)
        {
            outputLines.Add(("satisfies", string.Join(", ",
                needs.Select(n => $"{n.group.DisplayName}: {n.n.DisplayName}"))));
        }

        var enterable = b.Blueprint.GetComponent<ParsedEnterableSpec>();
        outputLines.Add(("Visitors", enterable.CapacityFinished.ToString()));
    }

    void ParseGoodConsuming(ParsedBuildingBlueprint b)
    {
        var gc = b.Blueprint.GetOptionalComponent<ParsedGoodConsumingBuildingSpec>();
        if (gc is null || gc.ConsumedGoods.Length == 0) { return; }

        outputLines.Add(("consumable", string.Join(", ", gc.ConsumedGoods.Select(g => g.GoodId))));
        outputLines.Add(("qty", string.Join(", ", gc.ConsumedGoods.Select(g => g.GoodPerHour.ToString("0.##")))));

        outputLines.Add(("storage", string.Join(", ", gc.ConsumedGoods.Select(g => GetStorage(g.GoodPerHour)))));

        outputLines.Add(("needs-haulers", "yes"));

        int GetStorage(float goodPerHour) 
            => (int)MathF.Round(gc.FullInventoryWorkHours * goodPerHour);
    }



}
