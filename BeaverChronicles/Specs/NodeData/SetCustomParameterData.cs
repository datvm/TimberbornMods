namespace BeaverChronicles.Specs.NodeData;

public record SetCustomParameterData
{
    public string Name { get; init; } = "";
    public CustomParameterValueType Type { get; init; } = CustomParameterValueType.String;
    public string Value1 { get; init; } = "";
    public string? Value2 { get; init; }
    public CustomParameterOperation Operation { get; init; } = CustomParameterOperation.Add;
}

public enum CustomParameterValueType
{
    String,
    Int,
    Float
}

public enum CustomParameterOperation
{
    Add,
    Sub,
    Mul,
    Div,
    Round,
    Ceil,
    Floor,
    Not
}
