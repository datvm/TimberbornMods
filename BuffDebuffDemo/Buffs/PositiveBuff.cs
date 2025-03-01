global using Timberborn.TimeSystem;

namespace BuffDebuffDemo.Buffs;

// This is a singleton IBuff instance. It is not applied to the entities (Buffable) directly but it provides some information about the buff.
// Think of it as a Manager for the buff. It is created once and then it creates the instances of the buff.
public class PositiveBuff(ISingletonLoader loader, IBuffService buffs, ILoc t, EventBus eb) : SimpleFloatBuff<PositiveBuff, PositiveBuffInstance>(loader, buffs), IUnloadableSingleton
{
    readonly IBuffService buffs = buffs;

    // The unique key to save and load the buff, in this case we only need to save the Id,
    // The logic is already implemented in the base class
    static readonly SingletonKey SaveKey = new("PositiveBuff");
    protected override SingletonKey SingletonKey => SaveKey;

    // The name and description of the buff
    public override string Name { get; protected set; } = t.T("LV.BuffDebuffDemo.PositiveBuff");
    public override string Description { get; protected set; } = t.T("LV.BuffDebuffDemo.PositiveBuffDesc");

    // Register and unregister this service for the event we need (DaytimeStartEvent)
    protected override void AfterLoad()
    {
        base.AfterLoad();
        eb.Register(this);

        #region Fix for initial run (In Practice 1, do not open this if you want to try)
        // Run once when the game is loaded for the first time
        // so player doesn't have to wait for the next day to get the buff
        if (!hadFirstRun)
        {
            OnDaytimeStart(new());
            hadFirstRun = true;
        }
        #endregion
    }

    public void Unload()
    {
        eb.Unregister(this);
    }

    // We update the buff instances when the day start
    [OnEvent]
    public void OnDaytimeStart(DaytimeStartEvent _)
    {
        const float MaxBuffPerc = 1f; // 100%;

        // Remove the previous buff
        // There should be only one but we remove all just in case
        // This logic can actually be processed with the BuffInstance as well if you want to
        // but in this case we keep it here so we don't have to store the day of the buff instance.
        var existing = buffs.GetInstances<PositiveBuffInstance>();
        foreach (var i in existing)
        {
            buffs.Remove(i);
        }

        // Add the new buff
        var perc = UnityEngine.Random.Range(0, MaxBuffPerc); // Get the random speed boost percentage
        var instance = CreateInstance(perc); // CreateInstance is implemented in the base class
        buffs.Apply(instance);

        // When developing, printing a trace is always helpful
        // But you should remove this in the final version
        Debug.Log($"Positive buff applied with {perc:P} speed boost");

        // In this demo I made it simple and have NegativeBuff and RandomBuff separately.
        // When you are used to this, you can actually make a single buff and create all 3 buff instances here.
        // However, all 3 buff instances will only get one name and description (though there is extra info at buff instance description and effects).
    }

    #region Fix for initial run (In Practice 1, do not open this if you want to try)

    bool hadFirstRun;
    static readonly PropertyKey<bool> hadFirstRunKey = new("PositiveBuffHadFirstRun");

    protected override void LoadSingleton(IObjectLoader loader)
    {
        base.LoadSingleton(loader);
        hadFirstRun = loader.Get(hadFirstRunKey);
    }

    protected override void SaveSingleton(IObjectSaver saver)
    {
        base.SaveSingleton(saver);
        saver.Set(hadFirstRunKey, hadFirstRun);
    }

    #endregion

    // Below is one way to process the buff instance when it is applied to the Buffable
    // However I do not recommend using this because there would be a huge amount of calls to these methods
    // including to entities that cannot receive this buff anyway
    // The recommended way in the guide is to use the events on the BuffableComponent instead (see BeaverBuffComponent)
    // using another decorated component because only the entities that have the component will receive the event
    //[OnEvent]
    //public void OnBuffApplied(BuffAddedToEntityEvent e)
    //{
    //    if (e.BuffInstance is PositiveBuffInstance i && i.Active)
    //    {
    //        var bonus = e.Buffable.GetComponentFast<BonusManager>();

    //        // See where Effect is in PositiveBuffInstance
    //        // Normally we would need to iterate through Effects but in this case we know there is only one
    //        var perc = i.Effect.Speed;

    //        // Add the bonus speed
    //        bonus.AddBonus(Constants.MovementSpeedBonusId, perc);
    //    }
    //}

    //[OnEvent]
    //public void OnBuffRemoved(BuffRemovedFromEntityEvent e)
    //{
    //    if (e.BuffInstance is PositiveBuffInstance i && i.Active)
    //    {
    //        var bonus = e.Buffable.GetComponentFast<BonusManager>();
    //        var perc = i.Effect.Speed;

    //        // Remove the bonus speed
    //        bonus.RemoveBonus(Constants.MovementSpeedBonusId, perc);
    //    }
    //}

}
