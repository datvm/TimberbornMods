namespace BenchmarkAndOptimizer.UI;

public class OptimizerPanel : VisualElement
{
    private readonly ILoc t;
    private readonly OptimizerSettings controller;

#nullable disable
    Toggle chkBm;

    VisualElement pnlWellKnown, pnlOthers;
    List<OptimizerItemElement> items;
#nullable enable

    VisualElement? pnlOptimizers;

    public OptimizerPanel(ILoc t, OptimizerSettings controller)
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
        chkBm.value = OptimizerSettings.EnableBenchmark;

        panel.AddGameLabel(t.T("LV.BO.EnableBenchmarkDesc"));

        var s = panel.style;
        s.borderBottomColor = Color.yellow;
        s.borderBottomWidth = 1;
    }

    void InitOptimizerUI()
    {
        var panel = this.AddChild();

        panel.AddMenuButton(t.T("LV.BO.Import"), onClick: OnImport);
        panel.AddMenuButton(t.T("LV.BO.Export"), onClick: controller.Export)
            .SetMarginBottom();

        InitList(panel);
    }

    void InitList(VisualElement parent)
    {
        pnlOptimizers?.RemoveFromHierarchy();

        pnlOptimizers = parent.AddChild();
        items = [];

        pnlWellKnown = AddPanel("LV.BO.WellKnownTasks");
        pnlOthers = AddPanel("LV.BO.OtherTasks");

        var wellknowns = OptimizableTypeService.WellKnownTypes;

        foreach (var t in OptimizableTypeService.OptimizableTypes)
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
            var el = pnlOptimizers.AddChild().SetMarginBottom();
            el.AddGameLabel(t.T(titleKey).Bold()).SetMarginBottom(10);
            return el;
        }
    }

    private void OnOptValueChanged(OptimizerItem value) => controller.SetOptimizer(value);

    void OnEnableBenchmarkChanged(bool value)
    {
        OptimizerSettings.EnableBenchmark = value;
    }

    void OnImport()
    {
        controller.Import();
        InitList(pnlOptimizers!.parent);
    }

}