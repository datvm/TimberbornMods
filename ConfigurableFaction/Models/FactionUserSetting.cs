namespace ConfigurableFaction.Models;

public class FactionUserSetting(string Id)
{

    public string Id { get; set; } = Id;
    public bool Clear { get; set; }

    public List<string> Buildings { get; } = [];
    public List<string> Plants { get; } = [];
    public List<string> Needs { get; } = [];
    public List<string> Goods { get; } = [];

}
