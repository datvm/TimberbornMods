namespace TImprove4Modders.DevModules;

public class BuildingsModule(
    EntityRegistry entities
) : IDevModule
{
    public DevModuleDefinition GetDefinition()
    {
        return new DevModuleDefinition.Builder()
            .AddMethod(DevMethod.Create("Buildings: Complete all Constructions", CompleteAllConstructions))
            .Build();
    }

    void CompleteAllConstructions()
    {
        foreach (var entity in entities.Entities)
        {
            var construction = entity.GetComponentFast<ConstructionSite>();
            if (construction)
            {
                construction.FinishNow();
            }
        }
    }

}
