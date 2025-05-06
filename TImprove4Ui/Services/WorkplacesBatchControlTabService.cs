namespace TImprove4Ui.Services;

public class WorkplacesBatchControlTabService(
    WorkplacesBatchControlTab tab,
    ILoc t
) : ILoadableSingleton, IUnloadableSingleton
{

    public static WorkplacesBatchControlTabService? Instance { get; private set; }

    bool showBeavers = true, showBots = true;

    public VisualElement CreateHeader()
    {
        var row = new VisualElement().SetAsRow();

        CreateFilter(row, 
            "Beaver.PluralDisplayName", 
            showBeavers, 
            (v) => showBeavers = v);
        CreateFilter(row,
            "Bot.PluralDisplayName",
            showBots,
            (v) => showBots = v);

        return row;
    }

    Toggle CreateFilter(VisualElement parent, string key, bool currValue, Action<bool> setValue)
    {
        var toggle = parent.AddToggle(key.T(t), onValueChanged: v =>
        {
            setValue(v);
            ApplyFilter();
        }).SetMarginRight();
        toggle.SetValueWithoutNotify(currValue);

        return toggle;
    }

    public void ApplyFilter()
    {
        var beavers = showBeavers;
        var bots = showBots;

        foreach (var grp in tab._rowGroups)
        {
            foreach (var row in grp._rows)
            {
                var workerType = row.Entity.GetComponentFast<WorkplaceWorkerType>();

                var visible = (workerType.WorkerType == WorkerTypeHelper.BeaverWorkerType && beavers)
                    || (workerType.WorkerType == WorkerTypeHelper.BotWorkerType && bots);
                row.Root.SetDisplay(visible);
            }
        }
    }

    public void Load()
    {
        Instance = this;
    }

    public void Unload()
    {
        Instance = null;
    }
}
