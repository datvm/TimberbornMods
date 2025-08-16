namespace WirelessCoil;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(WirelessCoil)).PatchAll();
    }

}
