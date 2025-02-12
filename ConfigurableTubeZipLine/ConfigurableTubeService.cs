global using Timberborn.BlockSystemNavigation;

namespace ConfigurableTubeZipLine;

public class ConfigurableTubeService : BaseComponent
{

    public void Awake()
    {
        var comp = GetComponentFast<BlockObjectNavMeshSettingsSpec>();
        if (comp?._edgeGroups is null) { return; }

        foreach (var e in comp._edgeGroups)
        {
            if (e is null) { continue; }
            var newCost = MSettings.CalculateCost(MSettings.TubewaySpeed);

            e._cost = newCost;
        }
    }

}
