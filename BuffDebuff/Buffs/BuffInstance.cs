namespace BuffDebuff;

[JsonObject(MemberSerialization.OptIn)]
public abstract class BuffInstance : IBuffEntity
{

    [JsonProperty]
    public long Id { get; set; }

    [JsonProperty]
    public string TypeName => GetType().FullName;

    public IBuff Buff { get; private set; } = null!;

    public abstract bool IsBuff { get; protected set; }
    public bool IsDebuff => !IsBuff;

    [JsonProperty]
    public bool Active { get; protected internal set; } = true;

    public abstract IEnumerable<IBuffTarget> Targets { get; }
    public abstract IEnumerable<IBuffEffect> Effects { get; }

    internal void SetBuff(IBuff buff)
    {
        Buff = buff;
    }

    public virtual void Init() { }
    public virtual void CleanUp() { }

    /// <summary>
    /// Load the instance state from a string (usually using a deserializer).
    /// </summary>
    /// <param name="savedState">The string containing the saved state of the instance.</param>
    /// <returns>True if the instance was successfully loaded and should be applied; false if the data is no longer valid or this instance does not need to be restored.</returns>
    internal protected virtual bool Load(string savedState)
    {
        JsonConvert.PopulateObject(savedState, this);
        return true;
    }

    /// <summary>
    /// Save the instance state into a string (usually using a serializer).
    /// </summary>
    /// <returns>A string data that can be used later to load the instance state. Null if the instance doesn't need to be saved.</returns>
    internal protected virtual string? Save()
    {
        return JsonConvert.SerializeObject(this);
    }

}
