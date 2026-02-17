namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record DecalSupplierSettingsModel(string Id, string Category);
public class DecalSupplierSettings(ILoc t) : BuildingSettingsBase<DecalSupplier, DecalSupplierSettingsModel>(t)
{
    public override string DescribeModel(DecalSupplierSettingsModel model) => model.Id ?? "";

    protected override bool ApplyModel(DecalSupplierSettingsModel model, DecalSupplier target)
    {
        if (model.Category != target.Category) { return false; }

        target.SetActiveDecal(new Decal(model.Id, model.Category));
        return true;
    }

    protected override DecalSupplierSettingsModel GetModel(DecalSupplier duplicable)
    {
        var active = duplicable.ActiveDecal;
        return new(active.Id, active.Category);
    }
}