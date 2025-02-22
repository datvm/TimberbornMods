namespace BuffDebuff;

public abstract class SimpleValueBuff<TValue, TBuff, TInstance> 
    : IBuff
    where TValue : notnull
    where TBuff : SimpleValueBuff<TValue, TBuff, TInstance>
    where TInstance : SimpleValueBuffInstance<TValue, TBuff, TInstance>
{
    public long Id { get; set; }

    public abstract string Name { get; }
    public abstract string Description { get; }

    /// <summary>
    /// Unfortunately static abstract member is not supported in .NET Standard yet, you need to recreate the instance using constructor here somehow
    /// </summary>
    protected abstract TInstance CreateInstance(IBuff buff, TValue value);

    public TInstance CreateInstance(TValue value)
    {
        return CreateInstance(this, value);
    }

}

public abstract class SimpleFloatBuff<TBuff, TInstance> 
    : SimpleValueBuff<float, TBuff, TInstance>
    where TBuff : SimpleValueBuff<float, TBuff, TInstance>
    where TInstance : SimpleValueBuffInstance<float, TBuff, TInstance>
{
}