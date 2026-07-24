namespace BuildingRenovations.UI;

[BindTransient]
public class BuildingRenovationMaterialRow : VisualElement
{
    readonly IconSpan good;
    readonly Label lblAmount;
    readonly IGoodService goods;
    readonly ILoc t;
    readonly ScienceService scienceService;
    readonly NamedIconProvider namedIconProvider;
    readonly Button btnPay;

    public event Action OnSciencePayClicked = null!;

    string? currGoodId;

    public bool Visible
    {
        get => this.IsDisplayed();
        set => this.SetDisplay(value);
    }

    public BuildingRenovationMaterialRow(IGoodService goods, ILoc t, ScienceService scienceService, NamedIconProvider namedIconProvider)
    {
        this.goods = goods;
        this.t = t;
        this.scienceService = scienceService;
        this.namedIconProvider = namedIconProvider;

        this.SetAsRow().AlignItems();

        good = this.AddIconSpan();
        this.AddChild().SetMarginLeftAuto();

        btnPay = this.AddGameButtonPadded(t.T("LV.BRe.Pay"), () => OnSciencePayClicked()).SetMarginRight(5);
        lblAmount = this.AddLabel();
    }

    public void SetContent(string id, int paid, int required)
    {
        var isScience = id == RenovationHelpers.ScienceId;

        if (currGoodId != id)
        {
            btnPay.SetDisplay(false);
            currGoodId = id;
            
            if (isScience)
            {
                good.SetContent(namedIconProvider.Science, null, t.T("LV.BRe.Science"), 24);
            }
            else
            {
                good.SetGood(goods, id, showName: true);
            }
        }

        var completed = paid >= required;
        string amountText;
        if (isScience)
        {
            if (completed)
            {
                btnPay.SetDisplay(false);
                amountText= $"{t.T("LV.BRe.Paid")} / {required}".Color(TimberbornTextColor.Green);
            }
            else
            {
                btnPay.SetDisplay(true);

                var points = scienceService.SciencePoints;
                var canPay = points >= required - paid;

                btnPay.enabledSelf = canPay;
                amountText = points.ToString().Color(canPay ? TimberbornTextColor.Green : TimberbornTextColor.Red);
                amountText += $" / {required}";
            }
        }
        else
        {
            amountText = $"{paid} / {required}";
            if (completed)
            {
                amountText = amountText.Color(TimberbornTextColor.Green);
            }
        }

        lblAmount.text = amountText;
        Visible = true;
    }

}
