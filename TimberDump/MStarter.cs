﻿namespace TimberDump;

public class MStarter : IModStarter
{
    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(TimberDump)).PatchAll();
    }
}
