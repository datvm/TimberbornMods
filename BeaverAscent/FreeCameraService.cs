using Timberborn.CameraSystem;

namespace BeaverAscent;

public class FreeCameraService(ModSettings s, CameraComponent camera) : ILoadableSingleton, IUnloadableSingleton
{
    public void Load()
    {
        camera.FreeMode = s.FreeCamera;
        s.FreeCameraChanged += S_FreeCameraChanged;
    }

    private void S_FreeCameraChanged(bool obj)
    {
        camera.FreeMode = obj;
    }

    public void Unload()
    {
        s.FreeCameraChanged -= S_FreeCameraChanged;
    }
}
