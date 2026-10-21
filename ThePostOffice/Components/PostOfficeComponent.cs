namespace ThePostOffice.Components;

[AddTemplateModule2(typeof(PostOfficeSpec))]
public class PostOfficeComponent : BaseComponent, IAwakableComponent
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

}
