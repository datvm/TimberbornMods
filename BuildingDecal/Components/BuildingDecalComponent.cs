
namespace BuildingDecal.Components;

public class BuildingDecalComponent(BuildingDecalProvider decalPictureService) : BaseComponent, IPersistentEntity, IAwakableComponent, IStartableComponent, IDuplicable<BuildingDecalComponent>
{
    static readonly ComponentKey SaveKey = new(nameof(BuildingDecalComponent));
    static readonly ListKey<BuildingDecalItem> DecalItemsKey = new("DecalItems");

#nullable disable
    BlockObject blockObject;
#nullable enable

    List<BuildingDecalItem> decalItems = [];
    public IReadOnlyList<BuildingDecalItem> DecalItems => decalItems;
    public Vector3Int BuildingSize => blockObject._blockObjectSpec.Size;

    void OnDecalsReloaded()
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
        decalPictureService.OnDecalsReloaded += OnDecalsReloaded;
        blockObject = GetComponent<BlockObject>();
    }

    public void Start()
    {
        var go = GameObject;
        foreach (var item in decalItems)
        {
            item.AttachTo(go);
            item.SetSprite(decalPictureService.GetSprite(item.DecalName));
        }
    }

    public BuildingDecalItem AddDecal(SpriteWithName sprite)
    {
        var decal = new BuildingDecalItem();
        decal.AttachTo(GameObject);
        decal.SetSprite(sprite);
        decalItems.Add(decal);

        return decal;
    }

    public BuildingDecalItem AddDecal(BuildingDecalItem copiedItem)
    {
        var sprite = decalPictureService.GetSprite(copiedItem.DecalName);
        var decal = AddDecal(sprite);

        decal.Position = copiedItem.Position;
        decal.Rotation = copiedItem.Rotation;
        decal.Scale = copiedItem.Scale;
        decal.Color = copiedItem.Color;
        decal.FlipX = copiedItem.FlipX;
        decal.FlipY = copiedItem.FlipY;

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

    public void SetTo(IReadOnlyCollection<BuildingDecalItem> items)
    {
        if (items.Count <= 0) { return; }

        SetTo([.. items.Select(SerializableDecalItem.From)]);
    }

    public bool SetTo(IReadOnlyCollection<SerializableDecalItem> items)
    {
        if (items.Count <= 0) { return false; }

        Clear();

        foreach (var item in items)
        {
            var sprite = decalPictureService.GetSprite(item.Name);
            var decal = AddDecal(sprite);

            decal.Position = item.Position;
            decal.Rotation = item.Rotation;
            decal.Scale = item.Scale;
            decal.Color = item.Color;
            decal.FlipX = item.FlipX;
            decal.FlipY = item.FlipY;
        }

        return true;
    }

    public void Clear()
    {
        while (decalItems.Count > 0)
        {
            RemoveDecal(decalItems[0]);
        }
    }

    public void DuplicateFrom(BuildingDecalComponent source) => SetTo(source.DecalItems);
}
