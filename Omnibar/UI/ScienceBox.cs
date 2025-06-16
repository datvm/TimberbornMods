namespace Omnibar.UI;

public class ScienceBox : VisualElement
{

    readonly Label lblScience, lblTotalScience;

    public int Science { get; private set; } 
    public int TotalScience { get; private set; }

    public ScienceBox()
    {
        var container = this;

        container.SetAsRow().AlignItems();

        container.AddChild(classes: ["science-cost-section__lock-icon"]);
        lblScience = container.AddGameLabel("0");
        container.AddGameLabel("/");
        lblTotalScience = container.AddGameLabel("0");
        container.AddChild(classes: ["science-cost-section__science-icon"]);
    }

    public void SetScience(int? science, int? totalScience)
    {
        if (science.HasValue)
        {
            Science = science.Value;
            lblScience.text = FormatScience(science.Value);
        }

        if (totalScience.HasValue)
        {
            TotalScience = totalScience.Value;
            lblTotalScience.text = FormatScience(totalScience.Value);
        }

        if (Science >= TotalScience)
        {
            lblScience.text = lblScience.text.Bold().Color(TimberbornTextColor.Green);
        }
    }

    public static string FormatScience(int science) => science.ToString("#,0");

}
