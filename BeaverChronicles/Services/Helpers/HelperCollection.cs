namespace BeaverChronicles.Services.Helpers;

[BindSingleton]
public class HelperCollection(
    FindEntityHelper findEntityHelper,
    FlagHelper flagHelper,
    GoodsHelper goodsHelper,
    CharacterSpawnHelper characterSpawnHelper,
    CharacterStatusHelper characterStatusHelper,
    GameStatHelper gameStatHelper,
    ILoc t
)
{
    public readonly ILoc t = t;

    public FindEntityHelper FindEntity => findEntityHelper;
    public FlagHelper Flags => flagHelper;
    public GoodsHelper Goods => goodsHelper;
    public CharacterSpawnHelper CharacterSpawn => characterSpawnHelper;
    public CharacterStatusHelper CharacterStatus => characterStatusHelper;
    public GameStatHelper GameStats => gameStatHelper;
}
