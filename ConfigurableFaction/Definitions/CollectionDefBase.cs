namespace ConfigurableFaction.Definitions;

public record CollectionDefBase<TItem>(string Id, ImmutableArray<TItem> Items);
