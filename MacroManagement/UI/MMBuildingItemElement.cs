namespace MacroManagement.UI;

public class MMBuildingItemElement : VisualElement
{

#nullable disable
    Label lblPause;

    public MMBuildingItem Item { get; private set; }
    MMBuildingItemElementInjection injection;
    Image icon;
    Label additionalInfo;
#nullable enable

    public MMBuildingItemElement SetItem(MMBuildingItem item, MMBuildingItemElementInjection injection)
    {
        Item = item;
        this.injection = injection;

        var row = this
            .SetPadding(0, 5)
            .AddRow()
            .AlignItems();

        var showingName = item.Name;
        if (item.DistrictName is not null)
        {
            showingName += Environment.NewLine + item.DistrictName;
        }

        var chkSelect = row.AddToggle(showingName, onValueChanged: Item.ToggleSelect)
            .SetFlexGrow(1)
            .SetMarginRight(5);
        item.OnSelectChanged += (_, select) => chkSelect.SetValueWithoutNotify(select);

        chkSelect.SetValueWithoutNotify(item.Select);

        additionalInfo = row.AddGameLabel().SetMarginRight(5);
        icon = row.AddImage().SetSize(20).SetMarginRight(5);
        lblPause = row.AddLabel();

        if (!item.SingleGoodAllower)
        {
            icon.SetDisplay(false);
        }

        UpdateData();

        return this;
    }

    public void UpdateData()
    {
        lblPause.text = Item.IsPaused ? "⏸️" : "▶️";

        var allower = Item.SingleGoodAllower;
        if (allower)
        {
            if (allower.HasAllowedGood)
            {
                var good = injection.GoodService.GetGood(allower.AllowedGood);
                icon.sprite = good.IconSmall.Value;
            }
            else
            {
                icon.sprite = injection.QuestionMark;
            }
        }

        var dwelling = Item.Dwelling;
        if (dwelling)
        {
            additionalInfo.text = $"{dwelling.NumberOfDwellers}/{dwelling.MaxBeavers}";
        }
    }

}

public readonly record struct MMBuildingItemElementInjection(
    IGoodService GoodService,
    Sprite QuestionMark
);