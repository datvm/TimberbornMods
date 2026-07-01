namespace TimberUi.Services;

public interface IModUpdateNotifier2
{

    string ModId { get; }
    string Version { get; }
    int VersionNumber { get; }
    string MessageLocKey { get; }

}
