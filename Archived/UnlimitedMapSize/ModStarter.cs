using System.Reflection;
using Timberborn.MapRepositorySystemUI;
using Timberborn.ModManagerScene;

namespace UnlimitedMapSize;
internal class ModStarter : IModStarter
{
    static readonly FieldInfo maxMapSizeField = typeof(NewMapBox).GetField("MaxMapSize", BindingFlags.NonPublic | BindingFlags.Static);

    public void StartMod(IModEnvironment modEnvironment)
    {
        maxMapSizeField.SetValue(null, int.MaxValue);
    }

}