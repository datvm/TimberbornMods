namespace TimberLive.Services;

public class CommonDataService : IApiConnectionListener
{
    readonly BlueprintApiService blueprintApi;

#nullable disable
    public FrozenDictionary<string, ParsedGoodSpec> Goods { get; private set; }
    public FrozenDictionary<string, ParsedNeedSpec> Needs { get; private set; }
    public FrozenDictionary<string, ParsedBonusTypeSpec> BonusTypes { get; private set; }
#nullable enable

    public CommonDataService(ApiService api, BlueprintApiService blueprintApi)
    {
        this.blueprintApi = blueprintApi;

        Clear();

        api.RegisterConnectedCallback(this);
    }

    public async Task OnConnectedAsync()
    {
        await Task.WhenAll([
            LoadGoods(),
            LoadNeeds(),
            LoadBonusTypes()
        ]).ConfigureAwait(false);

        async Task LoadGoods() => Goods = await blueprintApi.GetSpecsFrozenAsync<ParsedGoodSpec>();
        async Task LoadNeeds() => Needs = await blueprintApi.GetSpecsFrozenAsync<ParsedNeedSpec>();
        async Task LoadBonusTypes() => BonusTypes = await blueprintApi.GetSpecsFrozenAsync<ParsedBonusTypeSpec>();
    }

    public async Task OnDisconnectedAsync() => Clear();

    void Clear()
    {
        Goods = FrozenDictionary<string, ParsedGoodSpec>.Empty;
        Needs = FrozenDictionary<string, ParsedNeedSpec>.Empty;
        BonusTypes = FrozenDictionary<string, ParsedBonusTypeSpec>.Empty;
    }

}
