namespace TimberLive;

[EventHandler("onnodeclick", typeof(NodeClickEventArgs), enableStopPropagation: true, enablePreventDefault: false)]
public static class EventHandlers
{
}

public sealed class NodeClickEventArgs : EventArgs
{
    public string? EntityId { get; set; }
}
