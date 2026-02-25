namespace HueAndTurnX.Services;

public abstract class BaseHueTurnValueCopier<T, TModel>(ILoc t) : BuildingSettingsBase<T, TModel>(t)
    where T : BaseHueTurnValuesComponent<T, TModel>, IDuplicable<T>
    where TModel : SerializableModelBase
{
    public override string DescribeModel(TModel model) => "";

    protected override bool ApplyModel(TModel model, T target)
    {
        target.CopyFrom(model);
        return true;
    }

    protected override TModel GetModel(T duplicable) => duplicable.ToModel();
}

[MultiBind(typeof(IBuildingSettings), Contexts = BindAttributeContext.NonMenu)]
public class HueTurnColorCopier(ILoc t) : BaseHueTurnValueCopier<HueTurnColorComponent, SerializableColorModel>(t);

[MultiBind(typeof(IBuildingSettings), Contexts = BindAttributeContext.NonMenu)]
public class HueTurnPositionCopier(ILoc t) : BaseHueTurnValueCopier<HueTurnPositionComponent, SerializablePositionsModel>(t);