namespace PackagerBuilder.Services.GoodProviders;

public class Package10Provider(
    ILoc t,
    PackagerOverlayIconMaker iconMaker
) : IGoodBuilderProvider
{
    public const string PackagedGoodIdPrefix = "Packaged_";
    public const string PackageGoodLocKey = "LV.Pkg.Packaged__{0}";
    public const string PackageGoodPluralLocKey = "LV.Pkg.PluralPackaged__{0}";
    public const int PackagedGoodAmount = 10;
    public const string PackageRecipeId = "Package__{0}";
    public const string UnpackageRecipeId = "Unpackage__{0}";
    public const string PackageRecipeKey = "LV.Pkg.RecipePack__{0}";
    public const string UnpackageRecipeKey = "LV.Pkg.RecipeUnpack__{0}";
    public const string RecipeUnpackIcon = "RecipeUnpack__{0}";

    public AdditionalFactionData ProvideAdditionalData() => AdditionalFactionData.Empty;

    public bool ShouldProvide(PackagerBuildOptions options)
    {
        if (!options.Pack10) { return false; }

        iconMaker.MakeExportFolder(@"Sprites\Goods");
        iconMaker.MakeExportFolder(@"Sprites\Recipes");
        return true;
    }

    public IEnumerable<BuiltGood> ProvideGoods(GoodSpec g, PackagerBuildOptions options)
    {
        if (IsPackagedGood(g.Id)) { return []; }

        var packagedId = GetPackagedGoodId(g.Id);
        var name = t.T("LV.Pkg.PackagedName", g.DisplayName.Value);
        var pluralName = t.T("LV.Pkg.PackagedNamePlural", g.PluralDisplayName.Value);

        var packagedGood = g with
        {
            Id = packagedId,

            DisplayNameLocKey = string.Format(PackageGoodLocKey, g.Id),
            DisplayName = new(name),
            PluralDisplayNameLocKey = string.Format(PackageGoodPluralLocKey, g.Id),
            PluralDisplayName = new(pluralName),

            Weight = g.Weight * PackagedGoodAmount,
        };
        

        iconMaker.OverlayTo(g.Icon, PackagerOverlayIconMaker.OverlayType.Packaged, $@"Sprites\Goods\{packagedId}.png");

        return [new(
            g, packagedGood,
            new(
                [BuildRecipe(g, packagedId, true)],
                [BuildRecipe(g, packagedId, false)]
            )
        )];
    }

    BuiltGoodRecipe BuildRecipe(GoodSpec g, string packagedId, bool isPackaging)
    {
        var recipeId = GetPackagerRecipe(g.Id, isPackaging);

        string icon;
        if (isPackaging)
        {
            icon = @$"Sprites/Goods/{packagedId}";
        }
        else
        {
            icon = $"Sprites/Recipes/{GetUnpackageRecipeIcon(g.Id)}";
            iconMaker.OverlayTo(g.Icon, PackagerOverlayIconMaker.OverlayType.Unpacking, icon + ".png");
        }

        var nameKey = isPackaging
            ? string.Format(PackageRecipeKey, g.Id)
            : string.Format(UnpackageRecipeKey, g.Id);
        var name = isPackaging
            ? t.T("LV.Pkg.RecipePack", g.DisplayName.Value)
            : t.T("LV.Pkg.RecipeUnpack", g.DisplayName.Value);

        return new(
            new()
            {
                Id = recipeId,
                DisplayLocKey = nameKey,
                CycleDurationInHours = 1,
                CyclesCapacity = 10,
                Ingredients = [
                    new()
                    {
                        Id = isPackaging ? g.Id : packagedId,
                        Amount = isPackaging ? PackagedGoodAmount : 1,
                    }
                ],
                Products = [
                    new()
                    {
                        Id = isPackaging ? packagedId : g.Id,
                        Amount = isPackaging ? 1 : PackagedGoodAmount,
                    }
                ],
            },
            icon, name
        );
    }

    public static string GetPackagedGoodId(string id) => PackagedGoodIdPrefix + id;
    public static bool IsPackagedGood(string id) => id.StartsWith(PackagedGoodIdPrefix);
    public static string GetPackagerRecipe(string goodId, bool isPackager)
        => string.Format(isPackager ? PackageRecipeId : UnpackageRecipeId, goodId);
    public static string GetUnpackageRecipeIcon(string goodId) => string.Format(RecipeUnpackIcon, goodId);
    
}
