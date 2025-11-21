namespace ConfigurableTopBar.Services;

public class GoodSpriteProvider(AssetRefService assetRefService)
{
    public const string QuestionMarkPath = "ui/images/game/question-mark";

    readonly Dictionary<string, AssetRef<Sprite>> goodGroups = [];
    readonly Dictionary<string, AssetRef<Sprite>> goods = [];

    public IReadOnlyDictionary<string, AssetRef<Sprite>> GoodGroups => goodGroups;
    public IReadOnlyDictionary<string, AssetRef<Sprite>> Goods => goods;

    public void AddGoodGroup(AssetRef<Sprite> spriteRef)
    {
        goodGroups[spriteRef.Path] = spriteRef;
    }

    public void AddGood(AssetRef<Sprite> spriteRef)
    {
        goods[spriteRef.Path] = spriteRef;
    }

    public bool TryGetIcon(string path, out AssetRef<Sprite> sprite)
    {
        try
        {
            sprite = assetRefService.CreateAssetRef<Sprite>(path);
            return true;
        }
        catch (Exception)
        {
            sprite = GetQuestionMark();
            return false;
        }
    }

    public AssetRef<Sprite> GetQuestionMark() => assetRefService.CreateAssetRef<Sprite>(QuestionMarkPath);

}
