namespace PackagerBuilder.Services;

public class GoodBuilder(
    ISpecService spec,
    IEnumerable<IGoodBuilderProvider> providers,
    PackagerOverlayIconMaker iconMaker
)
{
    public const string GoodNameKey = "LV.Pkg.Good__{0}";
    public const string GoodPluralNameKey = "LV.Pkg.PluralGood__{0}";
    public const string RecipeNameKey = "LV.Pkg.RecipeName__{0}";

    string goodFolder = "";
    string recipeFolder = "";
    string prefabFolder = "";
    string factionFolder = "";
    string prefabGroupsFolder = "";

    readonly ImmutableArray<IGoodBuilderProvider> providers = [.. providers];

    public void Build(PackagerBuildOptions options, string modFolder, Dictionary<string, string> locs)
    {
        iconMaker.SetExportPath(modFolder);

        goodFolder = GetAndMakeFolder(modFolder, @"Blueprints\Goods");
        recipeFolder = GetAndMakeFolder(modFolder, @"Blueprints\Recipes");
        prefabFolder = GetAndMakeFolder(modFolder, @"Blueprints\ModdablePrefabs");
        factionFolder = GetAndMakeFolder(modFolder, @"Blueprints\Factions");
        prefabGroupsFolder = GetAndMakeFolder(modFolder, @"Blueprints\PrefabGroups");

        var providers = GetProviders(options);

        var goods = spec.GetSpecs<GoodSpec>();
        var builtGoods = goods
            .SelectMany(g => providers
                .SelectMany(prov => prov
                    .ProvideGoods(g, options)
                    .ToArray())
                .ToArray())
            .ToArray();

        WriteGoodBlueprints(builtGoods, locs);
        WriteRecipeBlueprints(builtGoods, locs);

        var additionalData = new AdditionalFactionData(providers
            .Select(q => q.ProvideAdditionalData()));
        BuildFactionList(builtGoods, additionalData);
    }

    ImmutableArray<IGoodBuilderProvider> GetProviders(PackagerBuildOptions options)
        => [.. providers.Where(q => q.ShouldProvide(options))];

    void WriteGoodBlueprints(BuiltGood[] builtGoods, Dictionary<string, string> locs)
    {
        foreach (var g in builtGoods)
        {
            var spec = g.BuiltSpec;
            var id = spec.Id;

            var nameKey = spec.DisplayNameLocKey;
            var pluralKey = spec.PluralDisplayNameLocKey;
            if (spec.DisplayName is not null)
            {
                locs[nameKey] = spec.DisplayName.Value;
            }
            if (spec.PluralDisplayName is not null)
            {
                locs[pluralKey] = spec.PluralDisplayName.Value;
            }


            var path = Path.Combine(goodFolder, $"{g.Id}.Good.json");
            var content = $$"""
{
  "GoodSpec": {
    "Id": "{{id}}",
    "BackwardCompatibleIds": [],
    "ConsumptionEffects": [],
    "DisplayNameLocKey": "{{nameKey}}",
    "PluralDisplayNameLocKey": "{{pluralKey}}",
    
    "GoodType": "{{spec.GoodType}}",
    "StockpileVisualization": "{{spec.StockpileVisualization}}",
    "VisibleContainer": "{{spec.VisibleContainer}}",
    "CarryingAnimation": "{{spec.CarryingAnimation}}",
    "Weight": {{spec.Weight}},
    "GoodGroupId": "{{spec.GoodGroupId}}",
    "Icon": "Sprites/Goods/{{id}}",
    "ContainerColor": "{{spec.ContainerColor}}",
    "ContainerMaterial": ""
  }
}
""";
            File.WriteAllText(path, content);
        }
    }

    void WriteRecipeBlueprints(BuiltGood[] builtGoods, Dictionary<string, string> locs)
    {
        foreach (var g in builtGoods)
        {
            foreach (var (r, icon, name) in g.Recipes.All)
            {
                var id = r.Id;

                var nameKey = string.Format(RecipeNameKey, id);
                locs[nameKey] = name;

                var path = Path.Combine(recipeFolder, $"{id}.Recipe.json");
                var content = $$"""
{
  "RecipeSpec": {
    "Id": "{{id}}",
    "DisplayLocKey": "{{nameKey}}",
    "CycleDurationInHours": 1,
    "CyclesCapacity": 10,
    "Ingredients": {{JsonConvert.SerializeObject(r.Ingredients)}},
    "Products": {{JsonConvert.SerializeObject(r.Products)}},
    "Icon": "{{icon}}",
    "BackwardCompatibleIds": []
  }
}
""";
                File.WriteAllText(path, content);
            }
        }
    }

    string GetAndMakeFolder(string modFolder, string path)
    {
        var result = Path.Combine(modFolder, path);
        Directory.CreateDirectory(result);
        return result;
    }

    void BuildFactionList(BuiltGood[] builtGoods, AdditionalFactionData additionalData)
    {
        foreach (var faction in spec.GetSpecs<FactionSpec>())
        {
            var filteredGoods = GetFactionFilteredList(faction, builtGoods);

            BuildFaction(faction, filteredGoods, additionalData);
            BuildFactionPrefabs(faction);
            BuildFactionRecipes(faction, filteredGoods, additionalData);
        }
    }

    void BuildFaction(FactionSpec faction, BuiltGood[] filteredGoods, AdditionalFactionData additionalData)
    {
        var goodIds = additionalData.GoodIds
            .Union(filteredGoods.Select(q => q.Id))
            .Distinct();

        var path = Path.Combine(factionFolder, $"Faction.{faction.Id}.json");
        var content = $$"""
{
  "FactionSpec": {
    "Goods#append": {{JsonConvert.SerializeObject(goodIds)}}
  }
}
""";
        File.WriteAllText(path, content);
    }

    void BuildFactionPrefabs(FactionSpec faction)
    {
        var id = faction.Id;

        var path = Path.Combine(prefabGroupsFolder, $"PrefabGroup.Buildings.{id}.optional.json");
        var content = $$"""
{
    "PrefabGroupSpec": {
        "Paths#append": [
            "{{PackagerPrefabProvider.PrefabPath}}{{PackagerPrefabProvider.PackagerName}}.{{id}}",
            "{{PackagerPrefabProvider.PrefabPath}}{{PackagerPrefabProvider.UnpackagerName}}.{{id}}"
        ]
    }
}
""";
        File.WriteAllText(path, content);
    }

    void BuildFactionRecipes(FactionSpec faction, BuiltGood[] filteredGoods, AdditionalFactionData additionalData)
    {
        var id = faction.Id;

        // Packager recipes
        BuildPackagerList(true);
        BuildPackagerList(false);

        // Other recipes
        if (additionalData.RecipesForBuildings.Count > 0)
        {
            foreach (var addition in additionalData.RecipesForBuildings)
            {
                WriteRecipe(addition.Id, addition.Prefabs, addition.Recipes);
            }
        }

        void BuildPackagerList(bool isPackager)
        {
            var prefabName = (isPackager ? PackagerPrefabProvider.PackagerName : PackagerPrefabProvider.UnpackagerName) + "."
                + id.ToLower().ToPascalCase(); // It's important because filename become lowercase

            var recipeIds = filteredGoods
                .SelectMany(q => q.Recipes.GetListIds(isPackager))
                .Union(additionalData.Recipes.GetListIds(isPackager));

            WriteRecipe(prefabName, [prefabName], recipeIds);
        }

        void WriteRecipe(string id, IEnumerable<string> prefabNames, IEnumerable<string> recipeIds)
        {
            var filePath = Path.Combine(prefabFolder, $"PackagerAddRecipes.{id}.PrefabModderSpec.json");
            var str = string.Join(", ", recipeIds.Select(q => $"'{q}'"));

            var content = $$"""
{
    "PrefabModderSpec": {
        "ComponentType": "Timberborn.Workshops.ManufactorySpec",
        "PrefabNames": {{JsonConvert.SerializeObject(prefabNames)}},
        "ValuePath": "_productionRecipeIds",
        "NewValue": "[{{str}}]",
        "AppendArray": true
    }
}
""";
            File.WriteAllText(filePath, content);
        }
    }

    BuiltGood[] GetFactionFilteredList(FactionSpec faction, BuiltGood[] builtGoods)
    {
        var factionGoods = faction.Goods.ToFrozenSet();

        return [.. builtGoods
            .Where(q => q.OriginalSpec is null || factionGoods.Contains(q.OriginalId))];
    }
}
