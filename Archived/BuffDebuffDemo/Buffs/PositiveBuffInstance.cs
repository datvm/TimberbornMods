namespace BuffDebuffDemo.Buffs;

public class PositiveBuffInstance : SimpleFloatBuffInstance<PositiveBuff>
{

    // Indicate this is a positive buff and will show up in the positie buff panel
    public override bool IsBuff { get; protected set; } = true;

    // These need to be set later
    public override IEnumerable<IBuffTarget> Targets { get; protected set; } = [];
    public override IEnumerable<IBuffEffect> Effects { get; protected set; } = [];

    // Here for this instance we know there is only one Effect of one kind so we give it directly
    // so we can easily access it later without iterating through the Effects
    // Note that we cannot init it here because Value is still not set when the class is created yet.
    public SpeedBuffEffect Effect { get; private set; } = null!;

    // We cannot inject them with constructor because this class must have a parameterless constructor
    // So we inject them with InjectAttribute instead
    IBuffableService buffables = null!;
    EventBus eb = null!;
    ILoc t = null!;

    // Add anything you need to inject here
    [Inject]
    public void Inject(IBuffableService buffables, EventBus eb, ILoc t)
    {
        this.buffables = buffables;
        this.eb = eb;
        this.t = t;
    }

    // This method is called after the BuffInstance was created and injected. Buff property and Value properties are set.
    // Init is also called when the game is loaded and the values are loaded from the save.
    public override void Init()
    {
        // We set the targets: GlobalBeaverBuffTarget and similar are already implemented by the system
        Targets = [new GlobalBeaverBuffTarget(buffables, eb)];
        
        // Then we add the speed effect:
        Effect = new SpeedBuffEffect(t, Value);
        Effects = [Effect];
    }

    // In case you need to modify the save and load logic, you can override these methods
    // The base class already saves and loads the Value property for you so in this case we do not need to modify it

    // If you do not want to save an instance (for example, you can always create it by the IBuff on load), return null
    // If you return null, the instance will not go into the game file
    protected override string? Save()
    {
        return base.Save();
    }

    // If you update your mod and no longer want to load a save, or the save data is not desired any more, return false
    // If you return false, the instance created will then be discarded.
    protected override bool Load(string savedState)
    {
        return base.Load(savedState);
    }

}
