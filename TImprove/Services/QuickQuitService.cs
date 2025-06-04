namespace TImprove.Services;

public class QuickQuitService(MSettings s) : IUpdatableSingleton, ILoadableSingleton
{
    bool quickQuit;

    public void UpdateSingleton()
    {
        if (quickQuit)
        {
            var kb = Keyboard.current;

            if (kb.qKey.isPressed && kb.ctrlKey.isPressed && kb.shiftKey.isPressed)
            {
                ForceQuit();
            }
        }
    }

    public void ForceQuit()
    {
        System.Diagnostics.Process.GetCurrentProcess().Kill();
    }

    public void Load()
    {
        s.ModSettingChanged += (_, _) => quickQuit = s.QuickQuit;
        quickQuit = s.QuickQuit;
    }

}
