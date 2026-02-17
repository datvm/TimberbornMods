namespace ModdableTimberborn.BuildingSettings;

public abstract class BuildingSettingsBase<T, TModel>(ILoc t) : IBuildingSettings<T, TModel>, ILoadableSingleton
    where T : BaseComponent, IDuplicable<T>
{
    protected readonly ILoc t = t;

    public string Name { get; private set; } = null!;

    protected abstract TModel GetModel(T duplicable);
    protected abstract bool ApplyModel(TModel model, T target);

    public virtual bool CanDeserialize(T? target) => target && target!.IsDuplicable;

    public void Load() => Name = t.T($"LV.MT.BldSet." + typeof(T).Name);

    public abstract string DescribeModel(TModel model);
    public virtual string DescribeDuplicable(T duplicable) => DescribeModel(GetModel(duplicable));

    public string Serialize(T duplicable) => JsonConvert.SerializeObject(GetModel(duplicable));

    public bool Deserialize(string settings, T target)
    {
        if (!CanDeserialize(target)) { return false; }

        var model = JsonConvert.DeserializeObject<TModel>(settings)!;
        return ApplyModel(model, target);
    }

}
