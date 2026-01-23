
namespace ConfigurableFaction.UI;

public interface IFactionItemsPanel
{

    event Action? OnItemChanged;

    IFactionItemsPanel Init(FactionOptions options, ImmutableArray<FactionInfo> otherFactions);
    void SetFilter(SettingsFilter filter);
    void RefreshItems();
}
