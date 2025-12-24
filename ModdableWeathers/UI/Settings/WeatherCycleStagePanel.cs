namespace ModdableWeathers.UI.Settings;

public class WeatherCycleStagePanel(
    ILoc t,
    WeatherCycleStageDefinitionService service
) : VisualElement
{

    public event EventHandler<bool> OnMoveRequested = null!;
    public event EventHandler<WeatherCycleStageDefinition> OnDeleteRequested = null!;

    public WeatherCycleStageDefinition Definition { get; private set; }
    public int Index { get; private set; }

    public void Init(WeatherCycleStageDefinition stage)
    {
        Definition = stage;
        Index = stage.Index;

        this.BorderAndSpace();

        var row = this.AddRow().AlignItems().SetMarginBottom();
        row.AddLabel(t.T("LV.MW.StageIndex", stage.Index + 1));
        row.AddChild().SetMarginLeftAuto();

        AddActionButton("LV.MW.StageRemove", () => OnDeleteRequested(this, Definition));
        AddActionButton("LV.MW.StageUp", () => OnMoveRequested(this, true));
        AddActionButton("LV.MW.StageDown", () => OnMoveRequested(this, false));

        AddSetting("LV.MW.StageSkipChance", stage.SkipChance, c => Definition with { SkipChance = c });
        AddSetting("LV.MW.StageBenignChance", stage.BenignChance, c => Definition with { BenignChance = c });
        AddSetting("LV.MW.LengthMul", stage.LengthMultiplier, c => Definition with { LengthMultiplier = c });

        void AddActionButton(string key, Action action)
        {
            row.AddGameButtonPadded(t.T(key), onClick: action).SetMarginRight(10);
        }

        void AddSetting(string key, int value, Func<int, WeatherCycleStageDefinition> onChange)
        {
            var panel = this.AddChild().SetMarginBottom();

            var row = panel.AddRow();
            row.AddLabel(t.T(key)).SetWidth(150).SetMarginRight();
            row.AddIntField(changeCallback: v => ChangeDefinition(onChange(v)))
                .SetFlexGrow()
                .SetValueWithoutNotify(value);

            panel.AddLabel(t.T(key + "Desc"));
        }
    }

    void ChangeDefinition(WeatherCycleStageDefinition definition)
    {
        Definition = definition;

        service.StagesDefinitions = [..service.StagesDefinitions
            .Select((d, i) => i == Index ? Definition : d)];
    }

}
