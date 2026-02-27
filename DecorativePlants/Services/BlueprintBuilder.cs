namespace DecorativePlants.Services;

public readonly record struct BuildOptions(bool AllFactions, bool Free, bool NoOccupation);

[BindSingleton(Contexts = BindAttributeContext.MainMenu)]
public class BlueprintBuilder(
    DialogService diag,
    ISpecService specs,
    ILoc t,
    BlueprintListingService blueprintListingService,
    IAssetLoader assetLoader,
    BlueprintSourceService blueprintSourceService
)
{
    const int CropOrderPadding = 10_000;
    const int FactionOrderPadding = 1_000_000;
    public const string ToolGroupId = "DecorativePlants";

    public void Build(BuildOptions options)
    {
        blueprintListingService.Initialize();
        InternalClear();

        var factions = specs.GetSpecs<FactionSpec>().ToArray();

        Dictionary<string, string>? parentGroups = null;
        if (options.AllFactions)
        {
            parentGroups = BuildBlockObjectToolGroup(factions);
        }

        foreach (var f in factions)
        {
            var fId = f.Id;

            List<GeneratedPlant> plants = [];
            HashSet<string> uniquePaths = [];
            HashSet<string> uniqueTemplateNames = [];

            var parentGroup = parentGroups?[fId] ?? ToolGroupId;
            MakePlantsForFaction(fId, fId, plants, uniquePaths, uniqueTemplateNames, 0, parentGroup);
            if (options.AllFactions)
            {
                var orderPadding = 0;
                foreach (var f2 in factions)
                {
                    var f2Id = f2.Id;

                    if (f2Id == f.Id) { continue; }
                    var f2ParentGroup = parentGroups?[f2Id] ?? ToolGroupId;

                    orderPadding += FactionOrderPadding;
                    MakePlantsForFaction(f2Id, fId, plants, uniquePaths, uniqueTemplateNames, orderPadding, f2ParentGroup);
                }
            }

            BuildTemplateCollection(fId, plants);
        }

        if (options.AllFactions)
        {
            BuildFactionMaterials(factions);
        }

        diag.Alert(t.T("LV.DP.GenerateDone", factions.Length));

        void MakePlantsForFaction(string fId, string originalFactionId, List<GeneratedPlant> plants, HashSet<string> uniquePaths, HashSet<string> uniqueTemplateNames, int orderPadding, string parentGroup)
        {
            var isOriginalFaction = fId == originalFactionId;
            var blueprintPaths = blueprintListingService.GetFactionBlueprints(fId);
            foreach (var path in blueprintPaths)
            {
                if (!uniquePaths.Add(path)) { continue; }

                var plant = MakeDecorativePlant(path, originalFactionId, isOriginalFaction, orderPadding, uniqueTemplateNames, parentGroup, options);

                if (plant is not null)
                {
                    plants.Add(plant.Value);
                }
            }
        }
    }

    Dictionary<string, string> BuildBlockObjectToolGroup(FactionSpec[] factions)
    {
        Dictionary<string, string> factionToGroupId = [];

        var order = 0;
        foreach (var f in factions)
        {
            var id = $"DecorativePlants.{f.Id}";
            factionToGroupId[f.Id] = id;

            var filePath = PersistentService.GetPlantGroupSpecFilePath(id);
            File.WriteAllText(filePath, $$"""
                {
                    "BlockObjectToolGroupSpec": {
                        "Id": "{{id}}",
                        "Order": {{++order}},
                        "NameLocKey": "{{f.DisplayNameLocKey}}",
                        "Icon": "{{f.Logo.Path}}"
                    },
                    "ParentToolGroupSpec": {
                        "ParentIds": [ "{{ToolGroupId}}" ]
                    }
                }
                """);
        }

        return factionToGroupId;
    }

    GeneratedPlant? MakeDecorativePlant(string path, string factionId, bool isOriginalFaction, int orderPadding, HashSet<string> uniqueTemplateNames, string toolGroupId, BuildOptions options)
    {
        var text = assetLoader.Load<BlueprintAsset>(path).Content;

        var src = JObject.Parse(text);
        var template = src[nameof(TemplateSpec)]?.Value<JObject>();
        if (template is null) { return null; }
        var srcTemplateName = template.GetProperty<string>(nameof(TemplateSpec.TemplateName));

        if (!uniqueTemplateNames.Add(srcTemplateName)) { return null; }

        var naturalResourceOrder = IsNaturalResource(src);

        if (naturalResourceOrder is null) { return null; }

        JObject dst = [];
        var templateName = $"DecorativePlants.{srcTemplateName}.{factionId}";

        MakeBuildingSpecs(src, dst, naturalResourceOrder.Value, templateName, options.Free, isOriginalFaction, orderPadding, toolGroupId);
        CopyBlockObjectSpec(src, dst, options.Free && options.NoOccupation);
        CopyLabel(src, dst);
        CopyModel(src, dst);

        var filePath = PersistentService.GetBuildingTemplateFilePath(templateName);
        File.WriteAllText(filePath, dst.ToString());

        return new(templateName, PersistentService.GetBuildingAssetPath(templateName));
    }

    static void MakeBuildingSpecs(JObject src, JObject dst, int order, string templateName, bool free, bool isOriginalFaction, int orderPadding, string toolGroupId)
    {
        var buildingSpec = dst[nameof(BuildingSpec)] = JObject.Parse($$"""
            {
                "SelectionSoundName": "Default",
                "LoopingSoundName": "",
                "BuildingCost": []
            }
            """);

        if (free)
        {
            buildingSpec[nameof(BuildingSpec.PlaceFinished)] = true;
        }
        else
        {
            ((JObject)buildingSpec).GetProperty<JArray>(nameof(BuildingSpec.BuildingCost)).Add(new JObject()
            {
                ["Id"] = isOriginalFaction ? GetYield(src) : DefaultYield,
                ["Amount"] = 1
            });
        }

        if (src.ContainsKey(nameof(CropSpec)))
        {
            order += CropOrderPadding;
        }
        order += orderPadding;

        dst[nameof(TemplateSpec)] = JObject.Parse($$"""
            {
                "TemplateName": "{{templateName}}",
                "BackwardCompatibleTemplateNames": [],
                "RequiredFeatureToggle": "",
                "DisablingFeatureToggle": ""
            }
            """);

        dst[nameof(DecorativePlantSpec)] = new JObject();

        dst[nameof(PlaceableBlockObjectSpec)] = JObject.Parse($$"""
            {
                "ToolGroupId": "{{toolGroupId}}",
                "ToolOrder": {{order}},
                "ToolShape": "Square",
                "Layout": "{{nameof(BlockObjectLayout.Rectangle)}}",
                "CustomPivot": {
                  "HasCustomPivot": false,
                  "Coordinates": {
                    "X": 0.0,
                    "Y": 0.0,
                    "Z": 0.0
                  }
                }
            }
            """);

        dst[nameof(BuildingModelSpec)] = JObject.Parse($$"""
            {
                "FinishedModelName": "#Finished",
                "UnfinishedModelName": "#Unfinished",
                "FinishedUncoveredModelName": "",
                "UndergroundModelName": "",
                "ConstructionModeModel": "Finished",
                "UndergroundModelDepth": 0
            }
            """);

    }

    static void CopyBlockObjectSpec(JObject src, JObject dst, bool noOccupation)
    {
        var bos = src.CopyProperty(dst, nameof(BlockObjectSpec));
        var blocks = bos.GetProperty<JArray>(nameof(BlockObjectSpec.Blocks));

        var isFirstBlock = true;
        foreach (var block in blocks.Cast<JObject>())
        {
            var matterBelow = Enum.Parse<MatterBelow>(block.GetProperty<string>(nameof(BlockSpec.MatterBelow)));
            if (matterBelow == MatterBelow.Ground)
            {
                block[nameof(BlockSpec.MatterBelow)] = MatterBelow.GroundOrStackable.ToString();
            }

            if (noOccupation)
            {
                block[nameof(BlockSpec.Occupations)] = isFirstBlock ? nameof(BlockOccupations.Floor) : nameof(BlockOccupations.None);
            }

            isFirstBlock = false;
        }
    }

    static void CopyLabel(JObject src, JObject dst)
    {
        var label = src.CopyProperty(dst, nameof(LabeledEntitySpec));
        label[nameof(LabeledEntitySpec.DescriptionLocKey)] = "LV.DP.DecorativePlantBuildingDesc";
    }

    static void CopyModel(JObject src, JObject dst)
    {
        var srcModelName = src.GetProperty<string>(nameof(BlockObjectModelSpec), nameof(BlockObjectModelSpec.FullModelName));

        var srcModel = src.GetProperty(nameof(Blueprint.Children)).FindChild<JObject>(srcModelName!)
            ?? throw new Exception($"Model {srcModelName} not found in children");

        var dstChildren = dst[nameof(Blueprint.Children)] = new JObject();
        dstChildren["#Unfinished"] = JObject.Parse("""
            {
              "Children": {
                "ConstructionBase1x1#nested": {
                  "BlueprintPath": "ConstructionBases/ConstructionBase1x1/ConstructionBase1x1.blueprint"
                },
              }
            }
            """);

        dstChildren["#Finished"] = (JObject)srcModel.DeepClone();
    }

    const string DefaultYield = "Log";
    static string GetYield(JObject src)
    {
        var cuttable = src[nameof(CuttableSpec)]?.Value<JObject>();
        if (cuttable is not null)
        {
            return cuttable.GetProperty<string>(
                nameof(CuttableSpec.Yielder),
                nameof(YielderSpec.Yield),
                nameof(GoodAmountSpec.Id)) ?? DefaultYield;
        }

        var gatherable = src[nameof(GatherableSpec)]?.Value<JObject>();
        if (gatherable is not null)
        {
            return gatherable.GetProperty<string>(
                nameof(GatherableSpec.Yielder),
                nameof(YielderSpec.Yield),
                nameof(GoodAmountSpec.Id)) ?? DefaultYield;
        }

        return DefaultYield;
    }

    static int? IsNaturalResource(JObject src) =>
        src[nameof(NaturalResourceSpec)]?.Value<JObject>()
        ?.GetProperty<int>(nameof(NaturalResourceSpec.Order));

    void BuildTemplateCollection(string fId, IEnumerable<GeneratedPlant> plants)
    {
        var collectionId = $"Buildings.{fId}";
        var filePath = PersistentService.GetTemplateCollectionPath(collectionId);

        var list = string.Join("," + Environment.NewLine, plants.Select(p => $"      \"{p.Path}\""));
        File.WriteAllText(filePath, $$"""
            {
              "TemplateCollectionSpec": {
                "CollectionId": "{{collectionId}}",
                "Blueprints": [
            {{list}}
                ]
              }
            }
            """);
    }

    void BuildFactionMaterials(FactionSpec[] factions)
    {
        var materials = factions.SelectMany(f => f.MaterialCollectionIds).Distinct();
        var materialList = string.Join("," + Environment.NewLine, materials.Select(m => $"      \"{m}\""));

        foreach (var f in factions)
        {
            var src = blueprintSourceService.Get(f.Blueprint);
            var filePath = PersistentService.GetFilePathAndPrepareFolder(src.Path);

            // Make it optional
            filePath = PersistentService.MakePathOptional(filePath);

            File.WriteAllText(filePath, $$"""
                {
                  "FactionSpec": {
                    "MaterialCollectionIds#append": [
                {{materialList}}
                    ]
                  }
                }
                """);
        }
    }

    public void Clear()
    {
        InternalClear();
        diag.Alert("LV.DP.ClearDone", true);
    }

    void InternalClear()
    {
        PersistentService.Clear();
    }

    readonly record struct GeneratedPlant(string TemplateName, string Path);
}
