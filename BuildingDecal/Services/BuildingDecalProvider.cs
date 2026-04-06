using UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

namespace BuildingDecal.Services;

public readonly record struct SpriteWithName(string Name, Sprite Sprite);

public class BuildingDecalProvider(
    IExplorerOpener explorerOpener,
    UserDecalTextureRepository userDecalTextureRepository,
    IDecalService decalService,
    DecalGroupService decalGroupService,
    EventBus eb
) : ILoadableSingleton
{
    public const string DecalCategory = "Banners";
    public const int PixelPerUnit = 256;
    public const string DefaultIconName = "";

    public event Action? OnDecalsReloaded;

    public SpriteWithName GetDefaultIcon() => GetSprite(DefaultIconName);

    public DecalCategoryGroups GetGroups() => decalGroupService.GetGroups(DecalCategory);

    public void Load()
    {
        eb.Register(this);
    }

    public void ReloadDecals()
    {
        decalService.ReloadCustomDecals(DecalCategory);
    }

    public void OpenFolder() => explorerOpener.OpenDirectory(userDecalTextureRepository.GetCustomDecalDirectory(DecalCategory));

    public SpriteWithName GetSprite(string name)
    {
        var decal = decalService.GetValidatedDecal(new(name, DecalCategory));
        return GetSprite(decal);
    }

    public SpriteWithName GetSprite(Decal decal)
    {
        var texture = decalService.GetDecalTexture(decal);

        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, PixelPerUnit);
        return new(decal.Id, sprite);
    }

    [OnEvent]
    public void OnDecalsReloadedEventBusEvent(DecalsReloadedEvent _) => OnDecalsReloaded?.Invoke();

}
