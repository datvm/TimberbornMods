namespace PackagerBuilder.Services.GoodProviders;

public class CrateGoodProvider(ILoc t, PackagerOverlayIconMaker iconMaker) : BaseTransformGoodProvider(t, iconMaker)
{
    public override string GoodIdPrefix { get; } = Package10Provider.PackagedGoodIdPrefix + "Crate_";
    public override string GoodLocKeyFormat { get; } = "LV.Pkg.Packaged__Crate__{0}";
    public override string GoodPluralLocKeyFormat { get; } = "LV.Pkg.PluralPackaged__Crate__{0}";
    public override string GoodNameLoc { get; } = "LV.Pkg.PackagedCrateName";
    public override string GoodPluralNameLoc { get; } = "LV.Pkg.PackagedCrateNamePlural";

    public override string GoodType { get; } = "Pileable";
    public override string ContainerItem { get; } = "BulkCrate";

    public override PackagerOverlayIconMaker.OverlayType PackingOverlayType { get; } = PackagerOverlayIconMaker.OverlayType.CratePackaged;
    public override PackagerOverlayIconMaker.OverlayType UnpackingOverlayType { get; } = PackagerOverlayIconMaker.OverlayType.CrateUnpacking;

    public override string PackageRecipeFormat { get; } = "Package__Crate__{0}";
    public override string UnpackageRecipeFormat { get; } = "Unpackage__Crate__{0}";
    public override string UnpackRecipeIconFormat { get; } = "Unpack__Crate__{0}";
    public override string PackageRecipeKeyFormat { get; } = "LV.Pkg.RecipePack__Crate__{0}";
    public override string UnpackageRecipeKeyFormat { get; } = "LV.Pkg.RecipeUnpack__Crate__{0}";
    public override string PackageRecipeLoc { get; } = "LV.Pkg.RecipePackCrate";
    public override string UnpackageRecipeLoc { get; } = "LV.Pkg.RecipeUnpackCrate";

    public override bool IsOptionEnabled(PackagerBuildOptions options) => options.Crate;
}
