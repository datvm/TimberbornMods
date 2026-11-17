namespace MechanizedManufactory.Services;

[BindSingleton]
public class LocalizationEntryAdder(ILoc t)
{

    readonly Loc t = (Loc)t;

    readonly Dictionary<string, string> kvs = [];

    public void Set(string key, string value) => kvs[key] = value;

    public void Commit()
    {
        var loc = t._localization;

        foreach (var (k,v) in kvs)
        {
            loc[k] = v;
        }

        kvs.Clear();
        t._localizationCache.Clear();
    }

}
