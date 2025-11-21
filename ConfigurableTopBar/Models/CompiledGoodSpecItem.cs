namespace ConfigurableTopBar.Models;

public readonly record struct CompiledGoodSpecItem(
    string GoodId,
    string GroupId,
    int Order
);