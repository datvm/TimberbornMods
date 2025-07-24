namespace BenchmarkAndOptimizer.UI;

public class OptimizerModSetting(OptimizerSettings optimizerSettings) : NonPersistentSetting(ModSettingDescriptor.Create(""))
{

    public override void Reset()
    {
        optimizerSettings.Clear();
    }

}

public class OptimizerModSettingFac(IContainer container) : IModSettingElementFactory
{
    public int Priority { get; }

    public bool TryCreateElement(ModSetting modSetting, out IModSettingElement? element)
    {
        if (modSetting is not OptimizerModSetting)
        {
            element = default;
            return false;
        }

        var el = container.GetInstance<OptimizerPanel>();
        element = new ModSettingElement(el, modSetting);
        return true;
    }
}
