namespace BeavlineLogistics.UI;

public class BeavlineNodePanel : CollapsiblePanel
{

    readonly Label lblFilter;
    readonly Toggle chkDisabled;
    readonly ILoc t;
    readonly IContainer container;
    readonly IGoodService goodService;

    FrozenSet<string> allGoods = [];
    FrozenSet<string> filteredGoods = [];

    public event Action<bool>? OnDisabledChanged;
    public event Action<FrozenSet<string>?>? OnFilterChanged;

    public BeavlineNodePanel(
        ILoc t,
        IContainer container,
        IGoodService goodService
    )
    {
        this.t = t;
        this.container = container;
        this.goodService = goodService;

        chkDisabled = Container.AddToggle(t.T("LV.BL.DisableToggle"),
            onValueChanged: v => OnDisabledChanged?.Invoke(v));
        lblFilter = Container.AddGameLabel("");
        Container.AddGameButton(t.T("LV.BL.ChangeFilter"), onClick: ChangeFilter)
            .SetPadding(0, 5);
    }

    public void SetContent(bool disabled, FrozenSet<string> allGoods, FrozenSet<string>? filteredGoods)
    {
        chkDisabled.SetValueWithoutNotify(disabled);

        this.allGoods = allGoods;
        this.filteredGoods = filteredGoods ?? [];
        lblFilter.text = t.T("LV.BL.FilterGoods", this.filteredGoods.Count == 0
            ? t.T("LV.BL.FilterAll")
            : string.Join(", ", filteredGoods.Select(q => goodService.GetGood(q).PluralDisplayName.Value))
        );

        this.SetDisplay(true);
    }

    public void ClearContent()
    {
        this.SetDisplay(false);
        allGoods = [];
        filteredGoods = [];
    }

    async void ChangeFilter()
    {
        var diag = container.GetInstance<GoodFilterDialog>();
        diag.SetContent(allGoods, filteredGoods);

        var result = await diag.ShowAsync();
        if (result is null) { return; }

        SetContent(chkDisabled.value, allGoods, result);
        OnFilterChanged?.Invoke(result.Count == 0 ? null : result);
    }

}
