namespace AutomationArrays.Components;

public enum RelayHubSingleMode
{
    And,
    Or,
    Index,
    Eq,
    Gt,
    Gte,
    Lt,
    Lte,
}

public enum RelayHubArrayMode
{
    Passthrough,
    Reverse,
    Repeat,
    Slice,
    Shift,
    
    And,
    Or,
    Xor,
}