namespace Omnibar.Services.Providers;

public class BuildingToolDescriptor(BlockObjectTool tool, IContainer container) : IOmnibarDescriptor
{
    readonly BuildingSpec buildingSpec = tool.Prefab.GetComponentFast<BuildingSpec>();

    public bool Describe(VisualElement el)
    {
        if (buildingSpec)
        {
            if (tool.Locker != null && buildingSpec.ScienceCost > 0)
            {
                var sciencePanel = el.AddRow().SetMarginRight();
                sciencePanel.style.alignItems = Align.Center;

                sciencePanel.AddChild(classes: ["science-cost-section__lock-icon"]);
                sciencePanel.AddGameLabel(buildingSpec.ScienceCost.ToString("#,0"));
                sciencePanel.AddChild(classes: ["science-cost-section__science-icon"]);
            }

            if (buildingSpec.BuildingCost?.Count > 0)
            {
                var fac = container.GetInstance<GoodItemFactory>();

                foreach (var cost in buildingSpec.BuildingCost)
                {
                    el.Add(fac.Create(cost));
                }
            }
        }

        if (tool.Prefab.DevModeTool)
        {
            el.AddGameLabel("THIS IS A DEV MODE TOOL".Color(TimberbornTextColor.Red));
        }

        return true;
    }

}
