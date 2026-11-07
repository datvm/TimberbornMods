namespace ThePostOffice.Tests;

sealed class OfficeWorld
{
    readonly Dictionary<string, GlobalGoodHandle> handles;

    OfficeWorld(
        EventBus events,
        PostOfficeGlobalInventoryService inventory,
        PostOfficeService offices,
        Dictionary<string, GlobalGoodHandle> handles)
    {
        Events = events;
        Inventory = inventory;
        Offices = offices;
        this.handles = handles;
    }

    public EventBus Events { get; }
    public PostOfficeGlobalInventoryService Inventory { get; }
    public PostOfficeService Offices { get; }

    public GlobalGoodHandle Handle(string id) => handles[id];

    public static OfficeWorld Create(int capacity = 10000, int parcelWeight = 60, params Good[] goods)
    {
        var events = new EventBus();
        events.PostLoad();

        Dictionary<string, GlobalGoodHandle> handles = [];
        List<GoodSpec> specs = [];
        foreach (var good in goods)
        {
            var spec = new GlobalGoodSpec
            {
                Id = good.Id,
                MinCapacity = 0,
                MaxCapacity = null,
                InitialAmount = good.Amount,
            };
            handles[good.Id] = new(spec, events);
            specs.Add(new()
            {
                Id = good.Id,
                Weight = good.Weight,
            });
        }

        var global = new GlobalInventoryService(null!, null!, events);
        var byId = typeof(GlobalInventoryService).GetField(
            "byId",
            BindingFlags.Instance | BindingFlags.NonPublic);
        byId!.SetValue(global, handles.ToFrozenDictionary());

        var inventory = new PostOfficeGlobalInventoryService(global, new CatalogGoods(specs), events);
        inventory.Load();
        if (capacity > 0)
        {
            inventory.AddCapacity(capacity);
        }

        var configs = new PostOfficeConfigService(new FixedParcelWeight(parcelWeight));
        configs.Load();
        var offices = new PostOfficeService(inventory, configs, null!, events, null!);
        return new(events, inventory, offices, handles);
    }

    sealed class CatalogGoods(List<GoodSpec> specs) : IGoodService
    {
        readonly List<string> ids = [.. specs.Select(spec => spec.Id)];
        readonly Dictionary<string, GoodSpec> byId = specs.ToDictionary(spec => spec.Id);

        public ReadOnlyList<string> Goods => new(ids);

        public bool HasGood(string id) => byId.ContainsKey(id);

        public GoodSpec GetGoodOrNull(string id) => byId.GetValueOrDefault(id)!;

        public GoodSpec GetGood(string id) => byId[id];

        public IEnumerable<string> GetGoodsForGroup(string groupId) => [];

        public IEnumerable<string> GetGoodsForType(string goodType) => [];
    }

    sealed class FixedParcelWeight(int parcelWeight) : ISpecService
    {
        public Blueprint GetBlueprint(string blueprintPath) => throw new NotSupportedException();

        public T GetSingleSpec<T>() where T : ComponentSpec
        {
            if (new PostOfficeConfigSpec { ParcelWeight = parcelWeight } is T spec)
            {
                return spec;
            }

            throw new InvalidOperationException($"No spec for {typeof(T).Name}.");
        }

        public IEnumerable<T> GetSpecs<T>() where T : ComponentSpec => [];
    }
}

readonly record struct Good(string Id, int Weight, int Amount = 0);
