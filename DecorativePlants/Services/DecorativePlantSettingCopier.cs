namespace DecorativePlants.Services;

public record DecorativePlantSettings(
    int MatureState,
    int WellnessState
);

[MultiBind(typeof(IBuildingSettings))]
public class DecorativePlantSettingCopier(ILoc t) : BuildingSettingsBase<DecorativePlantComponent, DecorativePlantSettings>(t)
{
    public override string DescribeModel(DecorativePlantSettings model)
        => $"{model.MatureState.TMature(t)}, {model.WellnessState.TWellness(t)}";

    protected override bool ApplyModel(DecorativePlantSettings model, DecorativePlantComponent target)
    {
        target.SetState((PlantMatureState)model.MatureState, (PlantWellnessState)model.WellnessState);
        return true;
    }

    protected override DecorativePlantSettings GetModel(DecorativePlantComponent duplicable)
        => new((int)duplicable.MatureState, (int)duplicable.WellnessState);

}
