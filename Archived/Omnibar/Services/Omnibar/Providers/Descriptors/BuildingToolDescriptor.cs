namespace Omnibar.Services.Omnibar.Providers.Descriptors;

public class BuildingToolDescriptor(BlockObjectTool tool, IContainer container) : IOmnibarDescriptor
{
    readonly BuildingSpec buildingSpec = tool.Prefab.GetComponentFast<BuildingSpec>();

    public bool Describe(VisualElement el)
    {
        if (buildingSpec)
        {
            var box = el.AddChild<BuildingCostBox>();

            if (tool.IsLocked())
            {
                var scienceService = container.GetInstance<ScienceService>();
                box.SetScience(scienceService.SciencePoints, buildingSpec.ScienceCost);
            }

            if (buildingSpec.BuildingCost?.Count > 0)
            {
                var fac = container.GetInstance<GoodItemFactory>();
                box.SetMaterials(buildingSpec.BuildingCost, fac);
            }
        }

        if (tool.Prefab.DevModeTool)
        {
            el.AddGameLabel("THIS IS A DEV MODE TOOL".Color(TimberbornTextColor.Red));
        }

        return true;
    }

}
