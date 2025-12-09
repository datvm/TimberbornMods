namespace Packager.Services;

public class PackagedGoodProvider(
    ILoc t,
    IAssetLoader assets,
    PackagerOverlayIconMaker iconMaker
) : ISpecModifier
{
    public const string PackageGoodId = "Packaged_{0}";
    public const string PackageGoodKey = "LV.Pkg.PackagedName_{0}";
    public const string PackageGoodPluralKey = "LV.Pkg.PackagedNamePlural_{0}";
    public const int PackagedGoodAmount = 10;
    public const string PackageRecipeId = "Package_{0}";
    public const string UnpackageRecipeId = "Unpackage_{0}";
    public const string PackageRecipeKey = "LV.Pkg.RecipePack_{0}";
    public const string UnpackageRecipeKey = "LV.Pkg.RecipeUnpack_{0}";

    public static ImmutableArray<string> OriginalGoodIds { get; private set; } = [];
    public static ImmutableArray<string> PackageGoodIds { get; private set; } = [];

    static ImmutableArray<GoodSpec>? packagedGoods;
    static ImmutableArray<RecipeSpec>? packagedRecipes;
    static ImmutableArray<RecipeSpec>? unpackagedRecipes;

    readonly Loc t = (Loc)t;

    public int Priority { get; }

    public T ModifyGetSingleSpec<T>(T current) where T : ComponentSpec => current;

    public IEnumerable<T> ModifyGetSpecs<T>(IEnumerable<T> current) where T : ComponentSpec
    {
        return current switch
        {
            IEnumerable<GoodSpec> goods => (IEnumerable<T>)OnGoodSpecArrived(goods),
            IEnumerable<RecipeSpec> recipes => (IEnumerable<T>)OnRecipeSpecArrived(recipes),
            IEnumerable<FactionSpec> factions => (IEnumerable<T>)OnFactionSpecArrived(factions),
            _ => current,
        };
    }

    public static string GetPackagerRecipe(string goodId, bool isPackager)
        => string.Format(isPackager ? PackageRecipeId : UnpackageRecipeId, goodId);

    public static string GetPackagedGoodId(string goodId) => string.Format(PackageGoodId, goodId);

    IEnumerable<GoodSpec> OnGoodSpecArrived(IEnumerable<GoodSpec> goods)
    {
        if (packagedGoods is null)
        {
            goods = InitializePackagedGoods(goods); // To make sure it can be iterated multiple times.
        }

        foreach (var good in goods)
        {
            yield return good;
        }

        foreach (var good in packagedGoods!.Value!)
        {
            yield return good;
        }
    }

    IEnumerable<RecipeSpec> OnRecipeSpecArrived(IEnumerable<RecipeSpec> recipes)
    {
        foreach (var r in recipes)
        {
            yield return r;
        }

        if (packagedRecipes is null) { yield break; } // Should not happen
        foreach (var r in packagedRecipes)
        {
            yield return r;
        }

        if (unpackagedRecipes is null) { yield break; } // Should not happen
        foreach (var r in unpackagedRecipes)
        {
            yield return r;
        }
    }

    IEnumerable<FactionSpec> OnFactionSpecArrived(IEnumerable<FactionSpec> factions)
    {
        foreach (var faction in factions)
        {
            var newGoodList = ConcatPackagedGoodsToFaction(faction.Goods);
            
            yield return faction with
            {
                Goods = newGoodList,
            };
        }
    }

    ImmutableArray<string> ConcatPackagedGoodsToFaction(IEnumerable<string> original)
    {
        List<string> list = [];
        foreach (var g in original)
        {
            list.Add(g);
            list.Add(GetPackagedGoodId(g));
        }

        return [.. list];
    }

    IEnumerable<GoodSpec> InitializePackagedGoods(IEnumerable<GoodSpec> goods)
    {
        goods = [.. goods]; // To ensure we can iterate multiple times.

        OriginalGoodIds = [.. goods.Select(q => q.Id).OrderBy(q => q)];

        packagedGoods = [..goods
            .Select(CreatePackagedGood)];
        PackageGoodIds = [.. packagedGoods.Value.Select(q => q.Id).OrderBy(q => q)];

        packagedRecipes = [..goods
            .Select(q => CreatePackagedRecipe(q, true))];
        unpackagedRecipes = [..goods
            .Select(q => CreatePackagedRecipe(q, false))];

        return goods;
    }

    RecipeSpec CreatePackagedRecipe(GoodSpec q, bool isPackage)
    {
        var nameKey = isPackage
            ? string.Format(PackageRecipeKey, q.Id)
            : string.Format(UnpackageRecipeKey, q.Id);
        var name = isPackage
            ? t.T("LV.Pkg.RecipePack", q.DisplayName.Value)
            : t.T("LV.Pkg.RecipeUnpack", q.DisplayName.Value);
        t._localization[nameKey] = name;

        ImmutableArray<GoodAmountSpecNew> unpacked = [new() { Id = q.Id, Amount = PackagedGoodAmount }];
        ImmutableArray<GoodAmountSpecNew> packed = [new() { Id = string.Format(PackageGoodId, q.Id), Amount = 1 }];

        var icon = iconMaker.Overlay(q.Icon);

        return new()
        {
            Id = GetPackagerRecipe(q.Id, isPackage),
            DisplayLocKey = nameKey,
            CycleDurationInHours = 1f,
            Ingredients = isPackage ? unpacked : packed,
            Products = isPackage ? packed : unpacked,
            UIIcon = new(icon),
            CyclesCapacity = 10,
            Icon = icon,
        };
    }

    GoodSpec CreatePackagedGood(GoodSpec q)
    {
        var nameKey = string.Format(PackageGoodKey, q.Id);
        var name = t.T("LV.Pkg.PackagedName", q.DisplayName.Value);

        var pluralNameKey = string.Format(PackageGoodPluralKey, q.Id);
        var pluralName = t.T("LV.Pkg.PackagedNamePlural", q.PluralDisplayName.Value);

        t._localization[nameKey] = name;
        t._localization[pluralNameKey] = pluralName;

        var icon = iconMaker.Overlay(q.Icon);

        return q with
        {
            Id = GetPackagedGoodId(q.Id),
            DisplayNameLocKey = nameKey,
            PluralDisplayNameLocKey = pluralNameKey,
            DisplayName = new(name),
            PluralDisplayName = new(pluralName),
            ConsumptionEffects = [],
            Weight = q.Weight * PackagedGoodAmount,
            Icon = icon,
            IconFlipped = new(icon),
            IconSmall = new(icon),
        };
    }
}
