namespace BuffDebuff;

[JsonObject(MemberSerialization.OptIn)]
public abstract class BuffInstance : IBuffEntity
{

    [JsonProperty]
    public long Id { get; set; }

    [JsonProperty]
    public string TypeName => GetType().FullName;

    public IBuff Buff { get; private set; } = null!;

    public virtual string? AdditionalDescription { get; protected set; }

    public abstract bool IsBuff { get; protected set; }
    public bool IsDebuff => !IsBuff;

    [JsonProperty]
    public bool Active { get; protected internal set; } = true;

    public abstract IEnumerable<IBuffTarget> Targets { get; protected set; }
    public abstract IEnumerable<IBuffEffect> Effects { get; protected set; }

    internal void SetBuff(IBuff buff)
    {
        Buff = buff;
    }

    public virtual void Init() { }
    public virtual void CleanUp() { }

    /// <summary>
    /// Run every game tick. Note that it is called even when <see cref="Active"/> is false.
    /// </summary>
    public virtual void Update() { }

    /// <summary>
    /// Load the instance state from a string (usually using a deserializer).
    /// </summary>
    /// <param name="savedState">The string containing the saved state of the instance.</param>
    /// <returns>True if the instance was successfully loaded and should be applied; false if the data is no longer valid or this instance does not need to be restored.</returns>
    internal protected virtual bool Load(string savedState)
    {
        Debug.Log($"Loading BuffInstance: {savedState}");

        JsonConvert.PopulateObject(savedState, this);
        return true;
    }

    /// <summary>
    /// Save the instance state into a string (usually using a serializer).
    /// </summary>
    /// <returns>A string data that can be used later to load the instance state. Null if the instance doesn't need to be saved.</returns>
    internal protected virtual string? Save()
    {
        var json = JsonConvert.SerializeObject(this);
        Debug.Log($"Saved BuffInstance: {json}");
        return json;
    }

}
