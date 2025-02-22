namespace BuffDebuff;

public abstract class SimpleValueBuffInstance<TValue, TBuff, TInstance>(TBuff buff, TValue value) 
    : BuffInstance<TBuff>(buff)
    where TValue : notnull
    where TBuff : SimpleValueBuff<TValue, TBuff, TInstance>
    where TInstance : SimpleValueBuffInstance<TValue, TBuff, TInstance>
{
    static readonly PropertyKey<string> ValueKey = new("Value");

    public TValue Value { get; internal protected set; } = value;

    protected override void LoadInstance(IObjectLoader loader)
    {
        base.LoadInstance(loader);

        var rawValue = loader.Get(ValueKey);
        Value = (TValue)Convert.ChangeType(rawValue, typeof(TValue));
    }

    protected override void SaveInstance(IObjectSaver saver)
    {
        base.SaveInstance(saver);

        saver.Set(ValueKey, Value.ToString());
    }

}

public abstract class SimpleFloatBuffInstance<TBuff, TInstance>(TBuff buff, float value) 
    : SimpleValueBuffInstance<float, TBuff, TInstance>(buff, value)
    where TBuff : SimpleValueBuff<float, TBuff, TInstance>
    where TInstance : SimpleValueBuffInstance<float, TBuff, TInstance>
{

}
