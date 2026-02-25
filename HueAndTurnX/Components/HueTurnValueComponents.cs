using UColor = UnityEngine.Color;

namespace HueAndTurnX.Components;

public abstract class BaseHueTurnValuesComponent<TSelf, TModel> : BaseComponent, IPersistentEntity, IDuplicable<TSelf>
    where TSelf : BaseHueTurnValuesComponent<TSelf, TModel>
    where TModel : SerializableModelBase
{
    static readonly ComponentKey SaveKey = new(nameof(HueTurnComponent));

    protected abstract PropertyKey<string> ValueKey { get; }
    public event Action OnValuesChanged = null!;

    public void CopyFrom(TModel model)
    {
        FromModel(model);
        OnValuesChanged();
    }

    protected abstract void FromModel(TModel model);
    public abstract TModel ToModel();

    public void Clear()
    {
        InternalClear();
        OnValuesChanged();
    }
    protected abstract void InternalClear();

    public void DuplicateFrom(TSelf source) => CopyFrom(source.ToModel());

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(ValueKey))
        {
            var json = s.Get(ValueKey);
            var model = JsonConvert.DeserializeObject<TModel>(json);

            if (model is not null)
            {
                FromModel(model);
            }
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        var model = ToModel();
        if (model.IsDefault) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(ValueKey, model.Serialize());
    }

    protected void RaiseValuesChanged() => OnValuesChanged();
}

[AddTemplateModule2(typeof(HueTurnComponent), Contexts = BindAttributeContext.NonMenu)]
public class HueTurnColorComponent : BaseHueTurnValuesComponent<HueTurnColorComponent, SerializableColorModel>
{
    static readonly PropertyKey<string> StaticValueKey = new("Colors");

    protected override PropertyKey<string> ValueKey => StaticValueKey;
    public UColor? Color { get; private set; }
    public float? Transparency { get; private set; }

    protected override void FromModel(SerializableColorModel model)
    {
        Color = model.Color;
        Transparency = model.Transparency;
    }

    public override SerializableColorModel ToModel() => new(Color, Transparency);

    protected override void InternalClear()
    {
        Color = null;
        Transparency = null;
    }

    public void SetValues(UColor? color = null, float? transparency = null)
    {
        if (color is not null)
        {
            Color = color;
        }

        if (transparency is not null)
        {
            Transparency = transparency.Value >= 0.995f ? null : transparency;
        }

        RaiseValuesChanged();
    }

    public void ClearColor()
    {
        Color = null;
        RaiseValuesChanged();
    }
}

[AddTemplateModule2(typeof(HueTurnComponent), Contexts = BindAttributeContext.NonMenu)]
public class HueTurnPositionComponent : BaseHueTurnValuesComponent<HueTurnPositionComponent, SerializablePositionsModel>
{
    static readonly PropertyKey<string> StaticValueKey = new("Positions");

    protected override PropertyKey<string> ValueKey => StaticValueKey;

    public Vector3? Rotation { get; private set; }
    public Vector3? Translation { get; private set; }
    public Vector3? Scale { get; private set; }

    protected override void FromModel(SerializablePositionsModel model)
    {
        Rotation = model.Rotation;
        Translation = model.Translation;
        Scale = model.Scale;
    }

    public override SerializablePositionsModel ToModel() => new(Rotation, Translation, Scale);

    public void SetValues(Vector3? rotation = null, Vector3? rotationPivot = null, Vector3? translation = null, Vector3? scale = null)
    {
        if (rotation is not null)
        {
            Rotation = rotation == default ? null : rotation;
        }

        if (translation is not null)
        {
            Translation = translation == default ? null : translation;
        }

        if (scale is not null)
        {
            if (scale == Vector3.one)
            {
                Scale = null;
            }
            else
            {
                Scale = scale == Vector3.one ? null : scale;
            }
        }

        RaiseValuesChanged();
    }

    protected override void InternalClear()
    {
        Rotation = null;
        Translation = null;
        Scale = null;
    }

}