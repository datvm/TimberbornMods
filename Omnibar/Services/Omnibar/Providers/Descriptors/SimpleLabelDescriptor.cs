namespace Omnibar.Services.Omnibar.Providers.Descriptors;

public class SimpleLabelDescriptor(string content) : IOmnibarDescriptor
{

    public bool Describe(VisualElement el)
    {
        el.AddGameLabel(content);
        return true;
    }

}
