namespace ConfigurableFaction.UI;

public class SettingDialog(
    ILoc t,
    VisualElementInitializer veInit,
    FactionInfoService info,
    FactionOptionsService options,
    DropdownItemsSetter cboSetter,
    IContainer container,
    IExplorerOpener opener
) : VisualElement
{

#nullable disable
    FactionSettingPanel factionPanel;
#nullable enable

    public SettingDialog Init()
    {
        options.Initialize();

        var container = this;

        var row = container.AddRow().SetMarginBottom(5);
        row.AddGameButton(t.T("LV.CFac.ShowFolder"), onClick: OpenStorageFolder)
            .SetPadding(paddingX: 5).SetMarginLeftAuto();

        container.AddLabel(t.T("LV.CFac.Faction"));
        var cboFactionSetter = AddFactionSelector(container);

        factionPanel = AddFactionPanel(container);

        this.Initialize(veInit);

        // Set dropdown only after init
        cboFactionSetter();

        return this;
    }

    Action AddFactionSelector(VisualElement parent)
    {
        var cbo = parent.AddDropdown()
            .SetFlexGrow(1)
            .AddChangeHandler((_, i) => OnFactionSelected(i))
            .SetMarginBottom();

        return () =>
        {
            var factions = info.FactionsInfo!.Factions.Select(q => q.Spec.DisplayName.Value).ToArray();
            cbo.SetItems(cboSetter, factions, factions[0]);
            OnFactionSelected(0);
        };
    }

    FactionSettingPanel AddFactionPanel(VisualElement parent)
    {
        var scrollView = parent.AddScrollView().SetFlexGrow(1);

        var factionPanel = container.GetInstance<FactionSettingPanel>();
        scrollView.Add(factionPanel);

        return factionPanel;
    }

    void OnFactionSelected(int index)
    {
        var faction = info.FactionsInfo!.Factions[index];
        factionPanel.SetFaction(faction);
    }

    void OpenStorageFolder() => opener.OpenDirectory(PersistentService.StoragePath);

}
