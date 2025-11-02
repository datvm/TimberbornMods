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

    static readonly FieldInfo VisibilityGetterBackingField = typeof(BatchControlRow).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
        .First(q => q.Name.Contains(nameof(BatchControlRow.VisibilityGetter)) && q.Name.Contains("BackingField"));
    public void OnBatchControlRowCreated(BatchControlRow row)
    {
        var workerType = row.Entity.GetComponentFast<WorkplaceWorkerType>();

        VisibilityGetterBackingField.SetValue(row, () =>
        {
            return (workerType.WorkerType == WorkerTypeHelper.BeaverWorkerType && showBeavers)
                || (workerType.WorkerType == WorkerTypeHelper.BotWorkerType && showBots);
        });
    }

    public void ApplyFilter()
    {
        tab.UpdateRowsVisibility();
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