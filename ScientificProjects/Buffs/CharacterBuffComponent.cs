
namespace ScientificProjects.Buffs;

public class CharacterBuffComponent : BaseComponent
{

    OneTimeUnlockProcessor processor = null!;

    [Inject]
    public void Inject(OneTimeUnlockProcessor processor)
    {
        this.processor = processor;
    }

    public void Start()
    {
        if (processor.HasWheelbarrows) // Use this so not every character has to check for wheelbarrows
        {
            ActivateWheelbarrow();
        }
        else
        {
            processor.OnWheelbarrowsUnlocked += ActivateWheelbarrow;
        }
    }

    void ActivateWheelbarrow()
    {
        var walker = GetComponentFast<WalkerSpeedManager>()
            ?? throw new MissingComponentException($"{nameof(WalkerSpeedManager)} component not found for Wheelbarrow activation");

        var spec = walker._walkerSpeedManagerSpec;
        spec._baseSlowedSpeed = spec._baseWalkingSpeed;
    }

}
