namespace ModdableTimberborn.BuildingSettings;

public interface IBuildingSettings
{
    int Order => 1000;
    Type Type { get; }
    Type ModelType { get; }

    string Name { get; }
    string Serialize(IDuplicable duplicable);
    string DescribeDuplicable(IDuplicable duplicable);
    string DescribeModel(object model);
    bool Deserialize(string settings, IDuplicable target);
    bool CanDeserialize(IDuplicable? target);

    IDuplicable? GetComponent(BaseComponent component) => component.GetComponent(Type) as IDuplicable;
}

public interface IBuildingSettings<T, TModel> : IBuildingSettings where T : BaseComponent, IDuplicable
{
    Type IBuildingSettings.Type => typeof(T);
    Type IBuildingSettings.ModelType => typeof(TModel);

    string Serialize(T duplicable);
    string IBuildingSettings.Serialize(IDuplicable duplicable) => Serialize((T)duplicable);

    string DescribeDuplicable(T duplicable);
    string IBuildingSettings.DescribeDuplicable(IDuplicable duplicable) => DescribeDuplicable((T)duplicable);

    string DescribeModel(TModel model);
    string IBuildingSettings.DescribeModel(object model) => DescribeModel((TModel)model);

    bool CanDeserialize(T? target);
    bool IBuildingSettings.CanDeserialize(IDuplicable? target) => CanDeserialize(target as T);

    bool Deserialize(string settings, T target);
    bool IBuildingSettings.Deserialize(string settings, IDuplicable target) => Deserialize(settings, (T)target);
}