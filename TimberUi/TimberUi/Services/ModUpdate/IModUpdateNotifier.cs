namespace TimberUi.Services;

public interface IModUpdateNotifier
{

    string ModId { get; }
    string Version { get; }
    string MessageLocKey { get; }

}
