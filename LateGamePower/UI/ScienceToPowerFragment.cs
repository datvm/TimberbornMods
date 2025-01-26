using Timberborn.BaseComponentSystem;
using Timberborn.EntityPanelSystem;

namespace LateGamePower.UI;

public class ScienceToPowerFragment(UIBuilder builder, ScienceToPowerService sciences) : IEntityPanelFragment
{
    VisualElement root = null!;
    SliderInt multiplication = null!;
    ScienceToPowerComponent? comp;

    public void ClearFragment()
    {
        SetVisibility(root, false);
    }

    public VisualElement InitializeFragment()
    {
        root = builder.BuildAndInitialize<PowerPanelFragment>();

        multiplication = root.Q<SliderInt>("PowerMultiplication");
        SetVisibility(root, false);
        return root;
    }

    public void ShowFragment(BaseComponent entity)
    {
        comp = entity.GetComponentFast<ScienceToPowerComponent>();
        
        var enabled = comp?.IsEnabled == true;
        if (enabled)
        {
            multiplication.value = sciences.Multiplication;
            multiplication.label = $"x{sciences.Multiplication}";
        }

        SetVisibility(root, enabled);
    }

    public void UpdateFragment()
    {
        var enabled = comp?.IsEnabled == true;
        if (!enabled) { return; }

        sciences.Multiplication = multiplication.value;
        multiplication.label = $"x{sciences.Multiplication}";
    }

    private static void SetVisibility(VisualElement element, bool visible)
    {
        element.visible = visible;
        element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
