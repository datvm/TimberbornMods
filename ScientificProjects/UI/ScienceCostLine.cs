namespace ScientificProjects.UI;

public class ScienceCostLine : VisualElement
{
    const int IconSize = 24;

    Label? lbl;
    public ScienceCostLine()
    {
        this.SetAsRow();
    }

    public ScienceCostLine AddIcon(Texture2D icon, string text)
    {
        this.AddImage(texture: icon, name: "Icon")
            .SetSize(IconSize, IconSize)
            .SetFlexShrink();

        this.AddGameLabel(text: text)
            .SetMarginRight(10)
            .SetFlexShrink(); 

        return this;
    }

    public ScienceCostLine SetCost(string msg)
    {
        lbl ??= this.AddGameLabel(name: "CostText").SetFlexGrow();

        lbl.text = msg;
        return this;
    }

}
