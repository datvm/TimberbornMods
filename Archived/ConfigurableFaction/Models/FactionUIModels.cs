namespace ConfigurableFaction.Models;

public class SettingsFilter
{
    public string Keyword { get; set; } = "";
    public bool ShowChecked { get; set; } = true;
    public bool ShowUnchecked { get; set; } = true;
    public bool HideSimilar { get; set; } = true;

}