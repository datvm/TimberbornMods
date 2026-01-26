namespace ConfigurableFaction.Models;

public record CollectionDefBase<TItem>(string Id, ImmutableArray<TItem> Items);
