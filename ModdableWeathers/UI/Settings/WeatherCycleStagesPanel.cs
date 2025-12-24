namespace ModdableWeathers.UI.Settings;

public class WeatherCycleStagesPanel : CollapsiblePanel
{
    readonly IContainer container;
    readonly WeatherCycleStageDefinitionService service;
    readonly VisualElement stageList;

    public WeatherCycleStagesPanel(ILoc t, IContainer container, WeatherCycleStageDefinitionService service)
    {
        this.container = container;
        this.service = service;

        SetTitle(t.T("LV.MW.CycleStages"));
        SetExpand(false);

        this.BorderAndSpace();

        var parent = Container;
        parent.AddLabel(t.T("LV.MW.CycleStagesDesc")).SetMarginBottom();

        parent.AddGameButtonPadded(t.T("LV.MW.StageAdd"), onClick: AddStage).SetMarginBottom();

        stageList = parent.AddChild();
        RefreshList();
    }

    void AddStage()
    {
        service.StagesDefinitions = [.. service.StagesDefinitions, new(service.StagesDefinitions.Length, 0, 100, 100)];
        RefreshList();
    }

    void RefreshList()
    {
        stageList.Clear();

        foreach (var stage in service.StagesDefinitions)
        {
            var el = container.GetInstance<WeatherCycleStagePanel>();
            el.Init(stage);

            el.OnMoveRequested += MoveStage;
            el.OnDeleteRequested += DeleteStage;

            stageList.Add(el);
        }
    }

    void DeleteStage(object sender, WeatherCycleStageDefinition e)
    {
        service.StagesDefinitions = [..service.StagesDefinitions
            .Where(x => x.Index != e.Index)
            .Select((x, i) => x with { Index = i })];
        RefreshList();
    }

    void MoveStage(object sender, bool up)
    {
        var arr = service.StagesDefinitions.ToArray();
        var count = arr.Length;

        var stage = ((WeatherCycleStagePanel)sender).Index;

        if ((up && stage == 0)
            || (!up && stage == count - 1)) { return; }

        var curr = arr[stage];
        var other = arr[up ? stage - 1 : stage + 1];

        arr[curr.Index] = other with { Index = curr.Index };
        arr[other.Index] = curr with { Index = other.Index };
        service.StagesDefinitions = [..arr];

        RefreshList();
    }
}
