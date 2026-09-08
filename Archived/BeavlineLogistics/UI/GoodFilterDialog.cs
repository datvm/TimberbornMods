namespace BeavlineLogistics.UI;

public class GoodFilterDialog(
    PanelStack panelStack,
    VisualElementInitializer veInit,
    IGoodService goodService,
    ILoc t
) : DialogBoxElement
{
    ImmutableArray<KeyValuePair<string, Toggle>> chkGoods;

    public void SetContent(FrozenSet<string> allGoods, FrozenSet<string> filteredGoods)
    {
        AddCloseButton();
        SetTitle(t.T("LV.BL.ChangeFilter"));

        SetDialogPercentSize(null, .75f);

        var pnlCommand = Content.AddRow().SetMarginBottom();

        pnlCommand.AddTextField(changeCallback: OnKeywordChanged)
            .SetFlexGrow(1).SetMarginRight(10);
        pnlCommand.AddGameButton(t.T("LV.BL.SelectAll"), onClick: () => SelectAll(true)).SetPadding(5, 5).SetMargin(marginX: 5);
        pnlCommand.AddGameButton(t.T("LV.BL.SelectNone"), onClick: () => SelectAll(false)).SetPadding(5, 5).SetMargin(marginX: 5);

        var list = Content.AddScrollView().SetMaxSizePercent(null, 100);
        List<KeyValuePair<string, Toggle>> chks = [];

        foreach (var id in allGoods)
        {
            var good = goodService.GetGood(id);
            var chk = list.AddToggle(good.PluralDisplayName.Value)
                .SetMarginBottom(5);
            chk.SetValueWithoutNotify(filteredGoods.Contains(id));
            chks.Add(new(id, chk));
        }

        chkGoods = [.. chks.OrderBy(q => q.Value.text)];
    }

    void OnKeywordChanged(string kw)
    {
        kw = kw.Trim();
        var noKw = kw.Length == 0;

        foreach (var (id, chk) in chkGoods)
        {
            var match = noKw || id.Contains(kw, StringComparison.OrdinalIgnoreCase) || chk.text.Contains(kw, StringComparison.OrdinalIgnoreCase);
            chk.SetDisplay(match);
        }
    }

    void SelectAll(bool check)
    {
        foreach (var chk in chkGoods)
        {
            chk.Value.SetValueWithoutNotify(check);
        }
    }

    public async Task<FrozenSet<string>?> ShowAsync()
    {
        await ShowAsync(veInit, panelStack);

        var ids = chkGoods
            .Where(q => q.Value.value)
            .Select(q => q.Key)
            .ToFrozenSet();

        return ids.Count == chkGoods.Length ? [] : ids;
    }

}
