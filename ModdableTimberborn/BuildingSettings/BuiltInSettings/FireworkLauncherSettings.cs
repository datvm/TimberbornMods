namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record FireworkLauncherSettingsModel(
    string? FireworkId,
    int Heading,
    int Pitch,
    int Distance,
    bool IsContinuous
);

public class FireworkLauncherSettings(
    ILoc t,
    FireworkSpecService fireworkSpecService
) : BuildingSettingsBase<FireworkLauncher, FireworkLauncherSettingsModel>(t)
{
    public override string DescribeModel(FireworkLauncherSettingsModel model)
        => GetOrFallback(model.FireworkId).DisplayName.Value;

    protected override bool ApplyModel(FireworkLauncherSettingsModel model, FireworkLauncher target)
    {
        if (model.FireworkId is not null && fireworkSpecService.HasSpec(model.FireworkId))
        {
            target.SetFireworkId(model.FireworkId);
        }

        target.SetHeading(model.Heading);
        target.SetPitch(model.Pitch);
        target.SetFlightDistance(model.Distance);
        target.SetContinuous(model.IsContinuous);

        return true;
    }

    FireworkSpec GetOrFallback(string? id)
    {
        if (string.IsNullOrEmpty(id) || !fireworkSpecService.HasSpec(id))
        {
            id = fireworkSpecService.GetFireworkIds()[0];
        }

        return fireworkSpecService.GetFireworkSpec(id);
    }

    protected override FireworkLauncherSettingsModel GetModel(FireworkLauncher target) 
        => new(target.FireworkId, target.Heading, target.Pitch, target.FlightDistance, target.IsContinuous);
}