namespace ConfigurableFaction.Models;

public class FactionUserSetting(string FactionId)
{

    public string FactionId { get; set; } = FactionId;
    public bool Clear { get; set; }

    public HashSet<string> Buildings { get; } = [];
    public HashSet<string> Plants { get; } = [];
    public HashSet<string> Needs { get; } = [];
    public HashSet<string> Goods { get; } = [];

    public void DeselectAll()
    {
        Buildings.Clear();
        Plants.Clear();
        Needs.Clear();
        Goods.Clear();
    }

}
