namespace PackagerBuilder.Services;

public class GoodBuilder(
    ISpecService spec,
    ILoc t,
    IAssetLoader assets
)
{
    public const string PackagedGoodIdPrefix = "Packaged_";
    public const string PackageGoodKey = "LV.Pkg.Packaged__{0}";
    public const string PackageGoodPluralKey = "LV.Pkg.PluralPackaged__{0}";
    public const int PackagedGoodAmount = 10;
    public const string PackageRecipeId = "Package__{0}";
    public const string UnpackageRecipeId = "Unpackage__{0}";
    public const string PackageRecipeKey = "LV.Pkg.RecipePack__{0}";
    public const string UnpackageRecipeKey = "LV.Pkg.RecipeUnpack__{0}";
    public const string RecipeUnpackIcon = "RecipeUnpack__{0}";

    string goodFolder = "";
    string iconFolder = "";
    string recipeFolder = "";
    string prefabFolder = "";
    string factionFolder = "";
    string prefabGroupsFolder = "";
    List<string> packingRecipes = [], unpackingRecipes = [];

    public void Build(string modFolder, Dictionary<string, string> locs)
    {
        packingRecipes = [];
        unpackingRecipes = [];

        goodFolder = GetAndMakeFolder(modFolder, @"Blueprints\Goods");
        iconFolder = GetAndMakeFolder(modFolder, @"Sprites\Goods");
        recipeFolder = GetAndMakeFolder(modFolder, @"Blueprints\Recipes");
        prefabFolder = GetAndMakeFolder(modFolder, @"Blueprints\ModdablePrefabs");
        factionFolder = GetAndMakeFolder(modFolder, @"Blueprints\Factions");
        prefabGroupsFolder = GetAndMakeFolder(modFolder, @"Blueprints\PrefabGroups");

        using var overlay = new PackagerOverlayIconMaker(assets);

        foreach (var g in spec.GetSpecs<GoodSpec>())
        {
            BuildGood(g, locs, overlay);
        }

        BuildFactionList();
    }

    string GetAndMakeFolder(string modFolder, string path)
    {
        var result = Path.Combine(modFolder, path);
        Directory.CreateDirectory(result);
        return result;
    }

    void BuildGood(GoodSpec g, Dictionary<string, string> locs, PackagerOverlayIconMaker overlay)
    {
        if (IsPackagedGood(g.Id)) { return; }

        var id = BuildGoodBlueprint(g, locs);
        BuildIcon(g, id, overlay);
        BuildRecipe(g, id, locs);
    }

    string BuildGoodBlueprint(GoodSpec g, Dictionary<string, string> locs)
    {
        var pgkGoodId = GetPackagedGoodId(g.Id);
        var nameKey = string.Format(PackageGoodKey, g.Id);
        var pluralKey = string.Format(PackageGoodPluralKey, g.Id);
        locs[nameKey] = t.T("LV.Pkg.PackagedName", g.DisplayName.Value);
        locs[pluralKey] = t.T("LV.Pkg.PackagedNamePlural", g.PluralDisplayName.Value);

        var path = Path.Combine(goodFolder, $"{pgkGoodId}.Good.json");
        var content = $$"""
{
  "GoodSpec": {
    "Id": "{{pgkGoodId}}",
    "BackwardCompatibleIds": [],
    "DisplayNameLocKey": "{{nameKey}}",
    "PluralDisplayNameLocKey": "{{pluralKey}}",
    "ConsumptionEffects": [],
    "GoodType": "{{g.GoodType}}",
    "StockpileVisualization": "{{g.StockpileVisualization}}",
    "VisibleContainer": "{{g.VisibleContainer}}",
    "CarryingAnimation": "{{g.CarryingAnimation}}",
    "Weight": {{g.Weight * PackagedGoodAmount}},
    "GoodGroupId": "{{g.GoodGroupId}}",
    "Icon": "Sprites/Goods/{{pgkGoodId}}",
    "ContainerColor": "{{g.ContainerColor}}",
    "ContainerMaterial": ""
  }
}
""";
        File.WriteAllText(path, content);

        return pgkGoodId;
    }

    void BuildIcon(GoodSpec g, string id, PackagerOverlayIconMaker overlay)
    {
        var path = Path.Combine(iconFolder, $"{id}.png");
        var overlayed = overlay.OverlayPackaged(g.Icon);
        File.WriteAllBytes(path, overlayed);

        var unpackPath = Path.Combine(iconFolder, $"{GetUnpackageRecipeIcon(g.Id)}.png");
        var unpackOverlayed = overlay.OverlayUnpacking(g.Icon);
        File.WriteAllBytes(unpackPath, unpackOverlayed);
    }

    void BuildRecipe(GoodSpec g, string id, Dictionary<string, string> locs)
    {
        packingRecipes.Add(BuildOneWayRecipe(true));
        unpackingRecipes.Add(BuildOneWayRecipe(false));

        string BuildOneWayRecipe(bool isPackaging)
        {
            var recipeId = GetPackagerRecipe(g.Id, isPackaging);
            var nameKey = isPackaging
                ? string.Format(PackageRecipeKey, g.Id)
                : string.Format(UnpackageRecipeKey, g.Id);
            var name = isPackaging
                ? t.T("LV.Pkg.RecipePack", g.DisplayName.Value)
                : t.T("LV.Pkg.RecipeUnpack", g.DisplayName.Value);
            locs[nameKey] = name;

            var icon = isPackaging ? id : GetUnpackageRecipeIcon(g.Id);

            var path = Path.Combine(recipeFolder, $"{recipeId}.Recipe.json");
            var content = $$"""
{
  "RecipeSpec": {
    "Id": "{{recipeId}}",
    "DisplayLocKey": "{{nameKey}}",
    "CycleDurationInHours": 1,
    "CyclesCapacity": 10,
    "Ingredients": [
      {
        "Id": "{{(isPackaging ? g.Id : id)}}",
        "Amount": {{(isPackaging ? 10 : 1)}}
      }
    ],
    "Products": [
      {
        "Id": "{{(isPackaging ? id : g.Id)}}",
        "Amount": {{(isPackaging ? 1 : 10)}}
      }
    ],
    "Icon": "Sprites/Goods/{{icon}}"
  }
}
""";
            File.WriteAllText(path, content);

            return recipeId;
        }
    }

    void BuildFactionList()
    {
        foreach (var faction in spec.GetSpecs<FactionSpec>())
        {
            BuildFaction(faction);
            BuildFactionPrefabs(faction);
            BuildFactionRecipes(faction);
        }
    }

    void BuildFaction(FactionSpec faction)
    {
        var goods = faction.Goods;

        var str = string.Join(", ", goods
            .Where(q => !IsPackagedGood(q))
            .Select(q => $"\"{GetPackagedGoodId(q)}\""));

        var path = Path.Combine(factionFolder, $"Faction.{faction.Id}.json");
        var content = $$"""
{
  "FactionSpec": {
    "Goods#append": [{{str}}]
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

    void BuildFactionRecipes(FactionSpec faction)
    {
        var id = faction.Id;

        BuildList(true);
        BuildList(false);

        void BuildList(bool isPackager)
        {
            var prefabName = (isPackager ? PackagerPrefabProvider.PackagerName : PackagerPrefabProvider.UnpackagerName) + "." 
                + id.ToLower().ToPascalCase(); // It's important because filename become lowercase

            List<string> list = [.. faction.Goods.Select(q => GetPackagerRecipe(q, isPackager))];
            var str = string.Join(", ", list.Select(q => $"'{q}'"));

            var filePath = Path.Combine(prefabFolder, $"AddRecipes.{prefabName}.{id}.ModdablePrefab.json");
            var content = $$"""
{
    "PrefabModderSpec": {
        "ComponentType": "Timberborn.Workshops.ManufactorySpec",
        "PrefabNames": [ "{{prefabName}}" ],
        "ValuePath": "_productionRecipeIds",
        "NewValue": "[{{str}}]"
    }
}
""";
            File.WriteAllText(filePath, content);
        }
    }

    public static string GetPackagedGoodId(string id) => PackagedGoodIdPrefix + id;
    public static bool IsPackagedGood(string id) => id.StartsWith(PackagedGoodIdPrefix);
    public static string GetPackagerRecipe(string goodId, bool isPackager)
        => string.Format(isPackager ? PackageRecipeId : UnpackageRecipeId, goodId);
    public static string GetUnpackageRecipeIcon(string goodId) => string.Format(RecipeUnpackIcon, goodId);

}
