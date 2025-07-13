namespace BenchmarkAndOptimizer.UI;

public class OptimizerPanel : VisualElement
{
    private readonly ILoc t;
    private readonly OptimizerSettingController controller;

#nullable disable
    Toggle chkBm;

    VisualElement optPanel, pnlWellKnown, pnlOthers;
    List<OptimizerItemElement> items;
#nullable enable

    public OptimizerPanel(ILoc t, OptimizerSettingController controller)
    {
        this.t = t;
        this.controller = controller;
        InitBenchmarkUI();
        InitOptimizerUI();
    }

    void InitBenchmarkUI()
    {
        var panel = this.AddChild()
            .SetPadding(bottom: 10)
            .SetMarginBottom(10);
        chkBm = panel.AddToggle(t.T("LV.BO.EnableBenchmark"), onValueChanged: OnEnableBenchmarkChanged);
        chkBm.value = OptimizerSettingController.EnableBenchmark;

        panel.AddGameLabel(t.T("LV.BO.EnableBenchmarkDesc"));

        var s = panel.style;
        s.borderBottomColor = Color.yellow;
        s.borderBottomWidth = 1;
    }

    void InitOptimizerUI()
    {
        optPanel = this.AddChild();
        if (MStarter.BenchmarkPatched)
        {
            ClearOptimizerUI();
            return;
        }

        InitList(optPanel);
    }

    void InitList(VisualElement parent)
    {
        var panel = parent.AddChild();
        items = [];

        pnlWellKnown = AddPanel("LV.BO.WellKnownTasks");
        pnlOthers = AddPanel("LV.BO.OtherTasks");

        var wellknowns = OptimizerSettingController.WellKnownTypes;
        
        foreach (var t in OptimizerSettingController.OptimizableTypes)
        {
            var isWellKnown = wellknowns.Contains(t);

            var target = isWellKnown ? pnlWellKnown : pnlOthers;
            var desc = isWellKnown ? this.t.T("LV.BO.WellKnown_" + t.Name) : null;
            var value = controller.GetValue(t);

            var el = target.AddChild<OptimizerItemElement>()
                .Init(t, value, desc: desc);

            el.OnValueChanged += OnOptValueChanged;

            items.Add(el);
        }

        VisualElement AddPanel(string titleKey)
        {
            var el = panel.AddChild().SetMarginBottom();
            el.AddGameLabel(t.T(titleKey).Bold()).SetMarginBottom(10);
            return el;
        }
    }

    private void OnOptValueChanged(OptimizerItem value)
    {
        if (value is null)
        {
            controller.RemoveOptimizer(value.Type);
        }
        else
        {
            controller.SetOptimizer(value);
        }
    }

    void ClearOptimizerUI()
    {
        optPanel.Clear();

        optPanel.AddGameLabel(t.T("LV.BO.OptimizerDisabled").Color(TimberbornTextColor.Red));
    }

    void OnEnableBenchmarkChanged(bool value)
    {
        OptimizerSettingController.EnableBenchmark = value;

        if (value)
        {
            ClearOptimizerUI();
        }
    }
}