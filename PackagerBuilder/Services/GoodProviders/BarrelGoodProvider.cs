namespace PackagerBuilder.Services.GoodProviders;

public class BarrelGoodProvider(ILoc t, PackagerOverlayIconMaker iconMaker) : BaseTransformGoodProvider(t, iconMaker)
{
    public override string GoodIdPrefix { get; } = Package10Provider.PackagedGoodIdPrefix + "Barrel_";
    public override string GoodLocKeyFormat { get; } = "LV.Pkg.Packaged__Barrel__{0}";
    public override string GoodPluralLocKeyFormat { get; } = "LV.Pkg.PluralPackaged__Barrel__{0}";
    public override string GoodNameLoc { get; } = "LV.Pkg.PackagedBarrelName";
    public override string GoodPluralNameLoc { get; } = "LV.Pkg.PackagedBarrelNamePlural";

    public override string GoodType { get; } = "Liquid";
    public override string ContainerItem { get; } = "LiquidBarrel";

    public override PackagerOverlayIconMaker.OverlayType PackingOverlayType { get; } = PackagerOverlayIconMaker.OverlayType.LiquidBarrelPackaged;
    public override PackagerOverlayIconMaker.OverlayType UnpackingOverlayType { get; } = PackagerOverlayIconMaker.OverlayType.LiquidBarrelUnpacking;

    public override string PackageRecipeFormat { get; } = "Package__Barrel__{0}";
    public override string UnpackageRecipeFormat { get; } = "Unpackage__Barrel__{0}";
    public override string UnpackRecipeIconFormat { get; } = "Unpack__Barrel__{0}";
    public override string PackageRecipeKeyFormat { get; } = "LV.Pkg.RecipePack__Barrel__{0}";
    public override string UnpackageRecipeKeyFormat { get; } = "LV.Pkg.RecipeUnpack__Barrel__{0}";
    public override string PackageRecipeLoc { get; } = "LV.Pkg.RecipePackBarrel";
    public override string UnpackageRecipeLoc { get; } = "LV.Pkg.RecipeUnpackBarrel";

    public override bool IsOptionEnabled(PackagerBuildOptions options) => options.Barrel;
}
