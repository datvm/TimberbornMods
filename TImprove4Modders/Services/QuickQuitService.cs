namespace TImprove4Modders.Services;

public class QuickQuitService : IUpdatableSingleton
{

    public void UpdateSingleton()
    {
        if (MSettings.QuickQuit || MSettings.QuickRestart)
        {
            var kb = Keyboard.current;

            if (kb.ctrlKey.isPressed && kb.shiftKey.isPressed)
            {
                if (MSettings.QuickQuit && kb.qKey.isPressed)
                {
                    ForceQuit();
                }

                if (MSettings.QuickRestart && kb.rKey.isPressed)
                {
                    ForceRestart();
                }
            }
        }
    }

    public void ForceRestart()
    {
        var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        System.Diagnostics.Process.Start(exePath);

        ForceQuit();
    }

    public void ForceQuit()
    {
        System.Diagnostics.Process.GetCurrentProcess().Kill();
    }

}
