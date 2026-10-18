namespace ThePostOffice.Components;

[AddTemplateModule2(typeof(PostOfficeSpec))]
public class PostOfficeComponent(PostOfficeService service) : BaseComponent, IAwakableComponent, IFinishedStateListener
{

#nullable disable
    public PostOfficeSpec PostOfficeSpec { get; private set; }
    public PostOfficeInventory PostOfficeInventory { get; private set; }
    public BlockObject BlockObject { get; private set; }
#nullable enable

    public void Awake()
    {
        PostOfficeSpec = GetComponent<PostOfficeSpec>();
        PostOfficeInventory = GetComponent<PostOfficeInventory>();
        BlockObject = GetComponent<BlockObject>();
    }

    public void OnEnterFinishedState() => service.OnFinished(this);
    public void OnExitFinishedState() => service.OnExitFinished(this);
}
