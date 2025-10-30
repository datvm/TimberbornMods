global using Timberborn.BlockSystemNavigation;

namespace ConfigurableTubeZipLine;

public class ConfigurableTubeService : BaseComponent, IAwakableComponent
{

    public void Awake()
    {
        var comp = GetComponent<BlockObjectNavMeshSettingsSpec>();
        if (comp?.EdgeGroups is null) { return; }

        foreach (var e in comp.EdgeGroups)
        {
            if (e is null) { continue; }
            var newCost = MSettings.CalculateCost(MSettings.TubewaySpeed);

            e.Cost = newCost;
        }
    }

}
