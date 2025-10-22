namespace PackagerBuilder.Services.GoodProviders;

public abstract class BaseTransformGoodProvider(
    ILoc t,
    PackagerOverlayIconMaker iconMaker
) : IGoodBuilderProvider
{
    public const int PackagedGoodAmount = Package10Provider.PackagedGoodAmount;
    public const int ContainerWeight = 15;
    public const string TransformedGoodType = "Box";
    public const string TransformedGoodGroup = "TransformedGood";

    public abstract string GoodIdPrefix { get; }
    public abstract string GoodLocKeyFormat { get; }
    public abstract string GoodPluralLocKeyFormat { get; }
    public abstract string GoodNameLoc { get; }
    public abstract string GoodPluralNameLoc { get; }

    public abstract string GoodType { get; }
    public abstract string ContainerItem { get; }
    public abstract PackagerOverlayIconMaker.OverlayType PackingOverlayType { get; }
    public abstract PackagerOverlayIconMaker.OverlayType UnpackingOverlayType { get; }

    public abstract string PackageRecipeFormat { get; }
    public abstract string UnpackageRecipeFormat { get; }
    public abstract string UnpackRecipeIconFormat { get; }
    public abstract string PackageRecipeKeyFormat { get; }
    public abstract string UnpackageRecipeKeyFormat { get; }
    public abstract string PackageRecipeLoc { get; }
    public abstract string UnpackageRecipeLoc { get; }

    public abstract bool IsOptionEnabled(PackagerBuildOptions options);

    public bool ShouldProvide(PackagerBuildOptions options)
    {
        if (!IsOptionEnabled(options)) { return false; }

        iconMaker.MakeExportFolder(@"Sprites\Goods");
        iconMaker.MakeExportFolder(@"Sprites\Recipes");

        return true;
    }

    public IEnumerable<BuiltGood> ProvideGoods(GoodSpec g, PackagerBuildOptions options)
    {
        if (g.GoodType != GoodType || Package10Provider.IsPackagedGood(g.Id)) { return []; }

        var packagedId = GetPackagedGoodId(g.Id);
        var name = t.T(GoodNameLoc, g.DisplayName.Value);
        var pluralName = t.T(GoodPluralNameLoc, g.PluralDisplayName.Value);

        var packagedGood = g with
        {
            Id = packagedId,
            
            DisplayNameLocKey = string.Format(GoodLocKeyFormat, g.Id),
            DisplayName = new(name),
            PluralDisplayNameLocKey = string.Format(GoodPluralLocKeyFormat, g.Id),
            PluralDisplayName = new(pluralName),

            GoodType = TransformedGoodType,
            GoodGroupId = TransformedGoodGroup,
            Weight = g.Weight * PackagedGoodAmount + ContainerWeight,

            VisibleContainer = VisibleContainer.Box,
            StockpileVisualization = "Box",
            ContainerColor = "#AB8D73",
            CarryingAnimation = "CarryInHands",
        };

        iconMaker.OverlayTo(g.Icon, PackingOverlayType, $@"Sprites\Goods\{packagedId}.png");

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
            var iconName = string.Format(UnpackRecipeIconFormat, g.Id);
            icon = $"Sprites/Recipes/{iconName}";
            iconMaker.OverlayTo(g.Icon, UnpackingOverlayType, icon + ".png");
        }

        var nameKey = isPackaging
            ? string.Format(PackageRecipeKeyFormat, g.Id)
            : string.Format(UnpackageRecipeKeyFormat, g.Id);
        var name = isPackaging
            ? t.T(PackageRecipeLoc, g.DisplayName.Value)
            : t.T(UnpackageRecipeLoc, g.DisplayName.Value);

        List<GoodAmountSpecNew> ingredients = [
            new()
            {
                Id = isPackaging ? g.Id : packagedId,
                Amount = isPackaging ? PackagedGoodAmount : 1,
            }
        ];

        List<GoodAmountSpecNew> products = [
            new()
            {
                Id = isPackaging ? packagedId : g.Id,
                Amount = isPackaging ? 1 : PackagedGoodAmount,
            }
        ];

        var containerList = isPackaging ? ingredients : products;
        containerList.Add(new()
        {
            Id = ContainerItem,
            Amount = PackagedGoodAmount,
        });

        return new(
            new()
            {
                Id = recipeId,
                DisplayLocKey = nameKey,
                CycleDurationInHours = 1,
                CyclesCapacity = 10,
                Ingredients = [.. ingredients],
                Products = [.. products],
            },
            icon, name
        );
    }

    public AdditionalFactionData ProvideAdditionalData() => new(
        [ContainerItem],
        BuiltRecipesPair.Empty,
        [
            new($"Add{GetType().Name}Recipe", ["WoodWorkshop.Folktails", "WoodWorkshop.IronTeeth"], [ContainerItem]),
        ]
    );

    string GetPackagedGoodId(string id) => GoodIdPrefix + id;
    string GetPackagerRecipe(string id, bool isPackaging) => isPackaging
        ? string.Format(PackageRecipeFormat, id)
        : string.Format(UnpackageRecipeFormat, id);

}
