namespace ScientificProjects.UI;

public class SPSciencePanel : VisualElement
{

    readonly Label lblNextDay, lblCurrent;
    readonly ILoc t;
    readonly ScientificProjectDailyService dailyService;
    readonly ScienceService scienceService;

    Button? btnPay;

    public event Action? OnDailyPaymentRequested;
    public event Action? OnSkipRequested;

    public SPSciencePanel(ILoc t, ScientificProjectDailyService dailyService, ScienceService scienceService)
    {
        this.t = t;
        this.dailyService = dailyService;
        this.scienceService = scienceService;

        var row = this.AddRow().SetMarginBottom(5);
        lblNextDay = row.AddGameLabel(name: "NextDayScience");
        lblCurrent = row.AddGameLabel(name: "CurrentScience")
            .SetMarginLeftAuto();

        this.AddGameLabel(text: "LV.SP.DayCostNotice".T(t));

        ReloadContent();
    }

    public void AddNotEnoughPanel()
    {
        var container = this.AddChild().SetMargin(marginY: 10);

        container.AddGameLabel(t.T("LV.SP.NotEnoughSciencePay"))
            .SetMarginBottom(5);

        var buttons = container.AddRow();
        btnPay = buttons.AddMenuButton(t.T("LV.SP.PayDayCost"), onClick: () => OnDailyPaymentRequested?.Invoke());
        buttons.AddMenuButton(t.T("LV.SP.SkipToday"), onClick: () => OnSkipRequested?.Invoke());

        ReloadContent();
    }

    public void ReloadContent()
    {
        var cost = dailyService.CalculateDayCost();
        lblNextDay.text = t.T("LV.SP.NextDayCost", cost.ToString("#,0"));

        var current = scienceService.SciencePoints;
        var currentFormat = current.ToString("#,0");
        var gainedYesterday = dailyService.ScienceGainedToday;
        var currText = gainedYesterday.HasValue
            ? t.T("LV.SP.CurrentSciencePlus", currentFormat, 
                (gainedYesterday > 0 ? "+" : "") + gainedYesterday)
            : t.T("LV.SP.CurrentScience", currentFormat);

        if (cost > current)
        {
            currText = currText.Color(TimberbornTextColor.Red);
        }
        lblCurrent.text = currText;

        btnPay?.enabledSelf = current >= cost;
    }

}
