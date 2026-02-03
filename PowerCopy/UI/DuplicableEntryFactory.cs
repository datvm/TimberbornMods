namespace PowerCopy.UI;

public class DuplicableEntryFactory
{
    readonly ILoc t;
    readonly IGoodService goods;
    readonly TemplateNameMapper templateMapper;
    readonly WorkerTypeHelper workerTypeHelper;
    readonly Dictionary<Type, Delegate> GetTextMethods = [];

    public DuplicableEntryFactory(
        ILoc t,
        IGoodService goods,
        TemplateNameMapper templateMapper,
        WorkerTypeHelper workerTypeHelper
    )
    {
        this.t = t;
        this.goods = goods;
        this.templateMapper = templateMapper;
        this.workerTypeHelper = workerTypeHelper;
        var methods = typeof(DuplicableEntryFactory).GetMethods(BindingFlags.Public | BindingFlags.Instance);
        foreach (var m in methods)
        {
            if (m.Name is nameof(GetText))
            {
                var parameters = m.GetParameters();
                var type = parameters[0].ParameterType;

                var func = Delegate.CreateDelegate(typeof(Func<,>).MakeGenericType(type, typeof(string[])), this, m);
                GetTextMethods[type] = func;
            }
        }
    }

    public DuplicableEntry CreateFor(Type type)
    {
        DuplicableEntry result;

        if (GetTextMethods.TryGetValue(type, out var func))
        {
            var entryType = typeof(DefaultUpdatableEntry<>).MakeGenericType(type);
            result = (DuplicableEntry)Activator.CreateInstance(entryType, func, t);
        }
        else
        {
            result = new DefaultDuplicableEntry(type, t);
        }

        result.Initialize();
        return result;
    }

    string YesNo(bool value) => t.T(value ? "Core.Yes" : "Core.No");

    public string[] GetText(PausableBuilding b)
        => [t.T(b.Paused ? "Status.Buildings.Paused" : "LV.PC.Unpaused")];
    public string[] GetText(BuilderPrioritizable b)
        => [GetPriorityName(b.Priority)];
    public string[] GetText(Demolishable b)
        => [YesNo(b.IsMarked)];
    public string[] GetText(FarmHouse d)
        => [t.T(d.PlantingPrioritized ? "Fields.Planting" : "Fields.Harvesting")];
    public string[] GetText(FlippableDecal d)
        => [YesNo(d.IsFlipped)];
    public string[] GetText(Forester d)
        => [YesNo(d.ReplantDeadTrees)];
    public string[] GetText(HaulPrioritizable d)
        => [YesNo(d.Prioritized)];
    public string[] GetText(SingleGoodAllower d)
        => [GetGoodName(d.AllowedGood)];
    public string[] GetText(GatherablePrioritizer d)
        => [GetGoodName(d.PrioritizedGatherable?.Yielder.Yield.Id)];
    public string[] GetText(PlantablePrioritizer d)
        => [GetTemplateName(d.PrioritizedPlantableSpec?.TemplateName)];
    public string[] GetText(StockpilePriority d)
        => [t.T("StockpilePriority." + GetStockpilePriorityKey(d))];
    public string[] GetText(FixedStockpileInventorySetter d)
        => [d._singleGoodAllower.AllowedGood, d._stockpile.Inventory.TotalAmountInStock.ToString()];
    public string[] GetText(Floodgate d)
        => [d.Height.ToString("F2"), YesNo(d.IsSynchronized)];
    public string[] GetText(WaterInputCoordinates d)
        => [d.UseDepthLimit ? d.DepthLimit.ToString() : t.T("Inventory.NothingSelected")];
    public string[] GetText(WaterMover d)
        => [t.T((d.CleanWaterMovement && d.ContaminatedWaterMovement)
            ? "WaterMover.Unfiltered"
            : (d.CleanWaterMovement ? "Good.Water.DisplayName" : "Good.Badwater.DisplayName"))];
    public string[] GetText(WaterSource d)
        => [d.SpecifiedStrength.ToString("F2")];
    public string[] GetText(WaterSourceRegulator d)
        => [YesNo(!d.IsOpen)];
    public string[] GetText(Manufactory d)
        => [t.T(d.CurrentRecipe?.DisplayLocKey ?? "Manufactory.NoRecipeOption")];
    public string[] GetText(DistrictDefaultWorkerType d)
        => [workerTypeHelper.GetDisplayText(d.WorkerType)];
    public string[] GetText(Workplace d)
        => [d.DesiredWorkers.ToString()];
    public string[] GetText(WorkplacePriority d)
        => [GetPriorityName(d.Priority)];
    public string[] GetText(WorkplaceWorkerType d)
        => [workerTypeHelper.GetDisplayText(d.WorkerType)];

    string GetGoodName(string? id) => id is null
        ? t.T("Planting.NoPriorityOption")
        : goods.GetGood(id).DisplayName.Value;

    string GetTemplateName(string? template) => template is null
        ? t.T("Planting.NoPriorityOption")
        : t.T(templateMapper.GetTemplate(template).GetSpec<LabeledEntitySpec>().DisplayNameLocKey);

    string GetPriorityName(Priority priority) => t.T("LV.PC.Priority_" + priority.ToString());

    static string GetStockpilePriorityKey(StockpilePriority d) =>
        d.IsAcceptActive ? "Accept" :
            (d.IsEmptyActive ? "Empty" :
            (d.IsObtainActive ? "Obtain" : "Supply"
        ));

}
