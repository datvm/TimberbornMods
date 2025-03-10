
namespace ScientificProjects.UI;

public class ScienceButton : NineSliceVisualElement
{

    readonly Label lblCost;

    public int Cost
    {
        set => lblCost.text = NumberFormatter.Format(value);
    }

    public ScienceButton()
    {
        var btn = this.AddGameButton().SetPadding(paddingX: 20);

        var container1 = btn.AddChild(name: "ScienceCostSection", classes: ["science-cost-section"]);
        var container2 = container1.AddChild<NineSliceVisualElement>(
            name: "ScienceCostBackground",
            classes: ["science-cost-section__background"]);

        container2.AddChild(classes: ["science-cost-section__lock-icon"]);
        lblCost = container2.AddGameLabel(text: "0", name: "ScienceCost", bold: true);

        container2.AddChild(classes: ["science-cost-section__science-icon"]);
    }

}
