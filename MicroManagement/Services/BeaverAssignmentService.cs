namespace MicroManagement.Services;

public class BeaverAssignmentService(BeaverPopulation beaverPopulation)
{
    public static readonly ImmutableArray<BeaverAssignmentTarget> AllTargetTypes =
        [.. Enum.GetValues(typeof(BeaverAssignmentTarget))
            .Cast<BeaverAssignmentTarget>()
            .OrderBy(q => q)];

    public void Assign(IEnumerable<Citizen> targets, DistrictCenter district)
    {
        var blockObj = district.GetComponentFast<BlockObject>();
        if (!blockObj || blockObj.PositionedEntrance is null)
        {
            throw new InvalidDataException("DistrictCenter has no entrance");
        }
        var coord = blockObj.PositionedEntrance.DoorstepCoordinates;

        foreach (var citizen in targets)
        {
            citizen.AssignDistrict(district);

            var walker = citizen.GetComponentFast<Walker>();
            walker.StopMoving();

            citizen.TransformFast.position = CoordinateSystem.GridToWorld(coord);
        }
    }

    public IEnumerable<Citizen> GetBeavers(BeaverAssignmentTarget targets)
    {
        foreach (var beaver in beaverPopulation._beaverCollection._beavers)
        {
            var needs = beaver.GetComponentFast<NeedManager>();

            switch (targets)
            {
                case BeaverAssignmentTarget.Sick:
                    if (needs && needs.NeedIsActive("BadwaterContamination"))
                    {
                        yield return GetCitizen(beaver);
                    }

                    break;
                case BeaverAssignmentTarget.Injured:
                    if (needs && needs.NeedIsActive("Injury"))
                    {
                        yield return GetCitizen(beaver);
                    }

                    break;
                case BeaverAssignmentTarget.Homeless:
                    var dweller = beaver.GetComponentFast<Dweller>();
                    if (dweller && !dweller.HasHome)
                    {
                        yield return GetCitizen(beaver);
                    }

                    break;
                case BeaverAssignmentTarget.Unemployed:
                    var worker = beaver.GetComponentFast<Worker>();
                    if (worker && !worker.Workplace)
                    {
                        yield return GetCitizen(beaver);
                    }

                    break;
                case BeaverAssignmentTarget.Stuck:
                    var citizen = GetCitizen(beaver);
                    if (citizen && !citizen.HasAssignedDistrict)
                    {
                        yield return citizen;
                    }

                    break;
            }
        }
    }

    static Citizen GetCitizen(BeaverSpec spec) => spec.GetComponentFast<Citizen>();

}

public enum BeaverAssignmentTarget
{
    Sick,
    Injured,
    Homeless,
    Unemployed,
    Stuck,
}
