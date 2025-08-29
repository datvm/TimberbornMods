namespace BuildingHP.UI;

public class RenovationDialog : DialogBoxElement
{
    readonly VisualElement contentPanel;
    BuildingRenovationComponent? comp;

    IRenovationProvider? currentProvider;
    VisualElement? currentUI;

    readonly PanelStack panelStack;
    readonly RenovationListView lstItems;
    readonly RenovationRegistry renovationRegistry;
    readonly ILoc t;

    readonly Label lblError;

    public RenovationDialog(RenovationRegistry renovationRegistry, VisualElementLoader veLoader, PanelStack panelStack, ILoc t)
    {
        this.panelStack = panelStack;
        this.t = t;
        this.renovationRegistry = renovationRegistry;

        SetTitle(t.T("LV.BHP.Renovations"));
        AddCloseButton();

        var el = Content;

        var row = el.AddRow();
        lstItems = row.AddChild<RenovationListView>()
            .SetWidth(300)
            .SetMarginRight();
        lstItems.RenovationSelected += OnRenovationSelected;

        contentPanel = row.AddChild().SetFlexGrow(1);

        lblError = this.AddGameLabel();
        lblError.RemoveFromHierarchy();

        this.Initialize(veLoader);
    }

    void OnRenovationSelected(RenovationProviderItemModel model)
    {
        currentProvider?.ClearUI(currentUI);
        contentPanel.Clear();
        currentUI = null;

        if (model.IsAvailable)
        {
            if (!comp) { return; }

            currentUI = model.Provider.CreateUI(comp);
            contentPanel.Add(currentUI);
        }
        else
        {
            lblError.text = model.NotAvailableReason!.Color(TimberbornTextColor.Red);
            contentPanel.Add(lblError);
        }
    }

    public async Task<bool> ShowAsync(BuildingRenovationComponent comp)
    {
        this.comp = comp;
        lstItems.Init(comp, renovationRegistry, t);

        var result = await ShowAsync(null, panelStack);

        currentProvider?.ClearUI(currentUI);
        currentProvider = null;
        currentUI = null;

        return result;
    }

}
