namespace TimberLive.Services;

public class CommonDataService : IApiConnectionListener
{
    readonly BlueprintApiService blueprintApi;

#nullable disable
    public FrozenDictionary<string, ParsedGoodSpec> Goods { get; private set; }
    public FrozenDictionary<string, ParsedNeedSpec> Needs { get; private set; }
    public FrozenDictionary<string, ParsedNeedGroupSpec> NeedsGroup { get; private set; }
    public FrozenDictionary<string, ParsedBonusTypeSpec> BonusTypes { get; private set; }
#nullable enable

    static readonly ImmutableArray<PropertyInfo> properties = [.. typeof(CommonDataService)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(FrozenDictionary<,>))];

    public CommonDataService(BlueprintApiService blueprintApi)
    {
        this.blueprintApi = blueprintApi;
        Clear();
    }

    public async Task OnConnectedAsync()
    {
        await Task.WhenAll([.. properties.Select(LoadPropertyAsync)]).ConfigureAwait(false);
    }

    static readonly MethodInfo GetSpecsFrozenMethod = typeof(BlueprintApiService).GetMethod(nameof(BlueprintApiService.GetSpecsFrozenAsync))!;
    async Task LoadPropertyAsync(PropertyInfo p)
    {
        var method = GetSpecsFrozenMethod?.MakeGenericMethod(p.PropertyType.GetGenericArguments()[1])!;

        var task = (Task)method.Invoke(blueprintApi, null)!;
        await task.ConfigureAwait(false);

        var resultProperty = task.GetType().GetProperty("Result")!;
        var result = resultProperty.GetValue(task);

        p.SetValue(this, result);
    }

    public async Task OnDisconnectedAsync() => Clear();

    void Clear()
    {
        foreach (var p in properties)
        {
            p.SetValue(this, null);
        }
    }

}
