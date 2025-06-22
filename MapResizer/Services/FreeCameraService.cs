namespace MapResizer.Services;

public class FreeCameraService(CameraService camera) : ILoadableSingleton
{
    public void Load()
    {
        camera.FreeMode = true;
    }
}
