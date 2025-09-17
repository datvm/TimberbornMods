namespace BuildingDecal.Components;

public class BuildingDecalComponent : BaseComponent, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(BuildingDecalComponent));
    static readonly ListKey<BuildingDecalItem> DecalItemsKey = new("DecalItems");

#nullable disable
    DecalPictureService decalPictureService;
    BlockObject blockObject;
#nullable enable

    List<BuildingDecalItem> decalItems = [];
    public IReadOnlyList<BuildingDecalItem> DecalItems => decalItems;
    public Vector3Int BuildingSize => blockObject.BlocksSpec.Size;

    [Inject]
    public void Inject(DecalPictureService decalPictureService)
    {
        this.decalPictureService = decalPictureService;
        decalPictureService.OnDecalsReloaded += DecalPictureService_OnDecalsReloaded;
    }

    private void DecalPictureService_OnDecalsReloaded()
    {
        foreach (var item in decalItems)
        {
            if (item.Attached)
            {
                item.SetSprite(decalPictureService.GetSprite(item.DecalName));
            }
        }
    }

    public void Awake()
    {
        blockObject = GetComponentFast<BlockObject>();
    }

    public void Start()
    {
        var go = GameObjectFast;
        foreach (var item in decalItems)
        {
            item.AttachTo(go);
            item.SetSprite(decalPictureService.GetSprite(item.DecalName));
        }
    }

    public BuildingDecalItem AddDecal(SpriteWithName sprite)
    {
        var decal = new BuildingDecalItem();
        decal.AttachTo(GameObjectFast);
        decal.SetSprite(sprite);
        decalItems.Add(decal);

        return decal;
    }

    public bool RemoveDecal(BuildingDecalItem item)
    {
        if (!decalItems.Remove(item)) { return false; }

        item.Detach();
        return true;
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        decalItems = s.Get(DecalItemsKey, BuildingDecalItemSerializer.Instance);
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (decalItems.Count <= 0) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(DecalItemsKey, decalItems, BuildingDecalItemSerializer.Instance);
    }
}
