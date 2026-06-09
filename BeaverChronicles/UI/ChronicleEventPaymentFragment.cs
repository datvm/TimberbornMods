namespace BeaverChronicles.UI;

[BindFragment]
public class ChronicleEventPaymentFragment(
    ScienceService science,
    IGoodService goods,
    NamedIconProvider namedIconProvider,
    ActiveChronicleEventService activeService,
    ILoc t,
    VisualElementInitializer veInit
) : IEntityPanelFragment
{

#nullable disable
    EntityPanelFragmentElement panel;
    IconSpan goodDisplay;
    Button btnPay;
    IntegerField txtAmount;
#nullable enable

    bool isScience;
    Inventory? inventory;
    string? currGoodId;
    bool showing;

    string? CurrentId => isScience ? ActiveEventPayment.ScienceId : currGoodId;

    GoodAmount showingGood;

    public VisualElement InitializeFragment()
    {
        panel = new() { Visible = false };

        var row = panel.AddRow().AlignItems().SetMarginBottom(10);
        row.AddLabel(t.T("LV.BCEv.Remaining")).SetMarginRight(5);
        goodDisplay = row.AddIconSpan();

        var amountRow = panel.AddRow().AlignItems().SetMarginBottom(5);
        amountRow.AddLabel(t.T("LV.BCEv.Amount")).SetMarginRight(5).SetFlexShrink(0);
        txtAmount = amountRow.AddIntField().SetFlexGrow().SetFlexShrink().SetMarginRight(5)
            .Initialize(veInit);
        amountRow.AddGameButtonPadded(t.T("LV.BCEv.Max"), SetToMax).SetFlexShrink(0);

        btnPay = panel.AddMenuButton(t.T("LV.BCEv.Pay"), Pay, stretched: true);

        return panel;
    }

    public void ClearFragment()
    {
        panel.Visible = showing = false;
        isScience = false;
        inventory = null;
        currGoodId = null;
        showingGood = default;
    }

    public void ShowFragment(BaseComponent entity)
    {
        if (!activeService.NeedsPayment) { return; }

        var sp = entity.GetComponent<Stockpile>();
        if (sp)
        {
            inventory = sp.Inventory;
            if (!ShowRemainingGoods(out _)) { return; }
        }
        else
        {
            isScience = IsScienceBuilding(entity);
            if (!ShowRemainingGoods(out _)) { return; }
        }

        panel.Visible = showing = true;
        SetToMax();
    }

    public void UpdateFragment()
    {
        var wasShowing = showing;
        if (!ShowRemainingGoods(out var remaining))
        {
            HidePanel();
            return;
        }

        if (!wasShowing)
        {
            SetToMax();
        }

        var min = Math.Min(remaining, GetPossessingAmount());

        if (txtAmount.value > min)
        {
            txtAmount.value = min;
        }

        btnPay.enabledSelf = CanPay();
        panel.Visible = showing = true;
    }

    bool ShowRemainingGoods(out int remaining)
    {
        if (inventory is not null)
        {
            currGoodId = GetPaymentGoodForInventory();
            if (currGoodId is not null && activeService.NeedToPay(currGoodId, out remaining))
            {
                SetDisplayedGood(currGoodId, remaining);
                return true;
            }
        }

        if (isScience && activeService.NeedToPayScience(out remaining))
        {
            SetDisplayedGood(ActiveEventPayment.ScienceId, remaining);
            return true;
        }

        remaining = 0;
        return false;
    }

    void HidePanel()
    {
        panel.Visible = showing = false;
        showingGood = default;
    }

    string? GetPaymentGoodForInventory()
    {
        var stockpileGood = inventory!.UnreservedStock().FirstOrDefault().GoodId ?? GetAllowedGoodId();
        if (stockpileGood is null || !activeService.NeedToPay(stockpileGood, out _))
        {
            return null;
        }

        return stockpileGood;

        string? GetAllowedGoodId()
        {
            return inventory!._goodDisallower is SingleGoodAllower { HasAllowedGood: true } allower
                ? allower.AllowedGood
                : null;
        }
    }

    void SetDisplayedGood(string id, int amount)
    {
        if (showingGood.GoodId == id && showingGood.Amount == amount) { return; }

        showingGood = new(id, amount);
        if (id == ActiveEventPayment.ScienceId)
        {
            goodDisplay.SetScience(namedIconProvider, amount.ToString()).SetVertical(false);

        }
        else
        {
            goodDisplay.SetGood(goods, id, amount.ToString(), true);
        }
    }

    void SetToMax()
    {
        int amount = 0;

        if (GetRemainingPayment(out var remaining))
        {
            amount = remaining;
        }

        if (amount > 0)
        {
            var possessingAmount = GetPossessingAmount();
            if (possessingAmount < amount)
            {
                amount = possessingAmount;
            }
        }

        txtAmount.value = amount;
    }

    void Pay()
    {
        if (!CanPay()) { return; }

        var amount = txtAmount.value;

        ConsumePayment(amount);
        activeService.Pay(CurrentId!, amount);

        UpdateFragment();
    }

    void ConsumePayment(int amount)
    {
        if (isScience)
        {
            science.SubtractPoints(amount);
        }
        else
        {
            inventory!.Take(new(currGoodId!, amount));
        }
    }

    bool GetRemainingPayment(out int remaining)
    {
        remaining = 0;

        var id = CurrentId;
        if (id is null) { return false; }

        return activeService.NeedsPayment && activeService.NeedToPay(id, out remaining);
    }

    bool CanPay()
    {
        var amount = txtAmount.value;
        if (amount <= 0) { return false; }

        var possessingAmount = GetPossessingAmount();
        if (amount > possessingAmount) { return false; }

        return GetRemainingPayment(out var remaining) && amount <= remaining;
    }

    int GetPossessingAmount()
    {
        if (isScience)
        {
            return science.SciencePoints;
        }

        if (inventory is not null && currGoodId is not null)
        {
            return inventory.UnreservedAmountInStock(currGoodId);
        }

        return 0;
    }

    static bool IsScienceBuilding(BaseComponent c)
    {
        var man = c.GetComponent<Manufactory>();
        return man && man.ProductionRecipes.FastAny(r => r.ProducesSciencePoints);
    }
}
