namespace TimberUi.Services;

[Obsolete($"Use {nameof(IModUpdateNotifier2)} instead. This interface does nothing.")]
public interface IModUpdateNotifier
{

    string ModId { get; }
    string Version { get; }
    string MessageLocKey { get; }

}
