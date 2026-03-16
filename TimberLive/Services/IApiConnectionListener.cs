namespace TimberLive.Services;

public interface IApiConnectionListener
{

    Task OnConnectedAsync();
    Task OnDisconnectedAsync();

}
