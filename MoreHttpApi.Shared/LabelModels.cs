namespace MoreHttpApi.Shared;

public record HttpLabeledEntity(string Display, string? ImagePath);
public record HttpNamedEntity(string EntityName, bool Editable);