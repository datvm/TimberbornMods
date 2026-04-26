namespace BeaverChronicles.Components;

[AddTemplateModule2(typeof(BeaverSpec))]
public class BeaverGrowTracker(EventBus eb) : BaseComponent, IChildhoodInfluenced
{
    
    public void InfluenceByChildhood(Character child)
    {
        var character = GetComponent<Character>();
        eb.Post(new BeaverGrownUpEvent(character, child));
    }

    public readonly record struct BeaverGrownUpEvent(Character Character, Character Child);
}
