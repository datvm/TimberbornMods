namespace ConfigurableFaction.UI;

public interface IEntryCollectionPanel
{
    IEnumerable<SettingEntryElement> Entries { get; }
}
