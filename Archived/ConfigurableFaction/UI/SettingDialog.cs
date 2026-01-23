namespace ConfigurableFaction.UI;

public class SettingDialog(
    ILoc t,
    VisualElementInitializer veInit,
    FactionInfoService info,
    FactionOptionsService options,
    DropdownItemsSetter cboSetter,
    IContainer container,
    IExplorerOpener opener,
    DialogBoxShower diag
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

        AddExportPanel(container);

        container.AddLabel(t.T("LV.CFac.Faction"));
        var cboFactionSetter = AddFactionSelector(container);

        factionPanel = AddFactionPanel(container);

        this.Initialize(veInit);

        // Set dropdown only after init
        cboFactionSetter();

        return this;
    }

    void AddExportPanel(VisualElement parent)
    {
        if (!MStarter.HasTimberModBuilder) { return; }

        var panel = parent.AddChild().SetMarginBottom();

        panel.AddMenuButton(t.T("LV.CFac.ExportToMod"),
            onClick: ExportToMod,
            size: UiBuilder.GameButtonSize.Large,
            stretched: true);
        panel.AddGameLabel(t.T("LV.CFac.ExportToModDesc"));
    }

    void ExportToMod()
    {
        var exportService = container.GetInstance<ExportToModService>();
        exportService.ExportToMod();

        diag.Create()
            .SetMessage(t.T("LV.CFac.ModExported", ExportToModService.ModName))
            .Show();
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
