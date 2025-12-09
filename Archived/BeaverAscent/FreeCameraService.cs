using Timberborn.CameraSystem;

namespace BeaverAscent;

public class FreeCameraService(
    ModSettings s,
#if TIMBER
    CameraComponent camera
#elif TIMBER7
    CameraService camera
#endif
) : ILoadableSingleton, IUnloadableSingleton
{
    public void Load()
    {
        camera.FreeMode = s.FreeCamera;
        s.FreeCameraChanged += FreeCameraChanged;
    }

    private void FreeCameraChanged(bool obj)
    {
        camera.FreeMode = obj;
    }

    public void Unload()
    {
        s.FreeCameraChanged -= FreeCameraChanged;
    }
}
