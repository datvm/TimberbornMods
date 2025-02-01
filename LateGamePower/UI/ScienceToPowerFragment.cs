global using TimberApi.UIBuilderSystem.CustomElements;
global using Timberborn.BaseComponentSystem;

namespace LateGamePower.UI;

public class ScienceToPowerFragment(UIBuilder builder, ScienceToPowerService sciences, ILoc loc, ModSettings s) : IEntityPanelFragment
{
#nullable disable
    VisualElement root = null!;
    bool enabled;

    NineSliceLabel lblCurr, lblTarget, lblPrev, lblNext, lblNoScience;
    SliderInt multiplication;
#nullable enable

    public void ClearFragment()
    {
        SetVisibility(root, false);
    }

    public VisualElement InitializeFragment()
    {
        root = builder.BuildAndInitialize<PowerPanelFragment>();

        lblCurr = root.Q<NineSliceLabel>("CurrentMul");
        lblTarget = root.Q<NineSliceLabel>("SetMul");
        lblPrev = root.Q<NineSliceLabel>("PrevCost");
        lblNext = root.Q<NineSliceLabel>("NextCost");
        lblNoScience = root.Q<NineSliceLabel>("NoScience");

        multiplication = root.Q<SliderInt>("TargetMul");
        SetVisibility(root, false);
        return root;
    }

    public void ShowFragment(BaseComponent entity)
    {
        enabled = entity.GetComponentFast<MechanicalNode>()?.IsGenerator == true;
        if (enabled)
        {
            multiplication.highValue = s.MaxMultiplier;
            multiplication.value = sciences.TargetMultiplication;

            UpdateLabels();
        }

        SetVisibility(root, enabled);
    }

    public void UpdateFragment()
    {
        if (!enabled) { return; }

        sciences.TargetMultiplication = multiplication.value;
        UpdateLabels();
    }

    void UpdateLabels()
    {
        var curr = sciences.CurrentMultiplication;

        lblCurr.text = loc.T("LV.LGP.CurrentMul", curr, sciences.CalculateScienceCost(curr));
        
        var target = sciences.TargetMultiplication;
        lblNoScience.visible = sciences.NotEnoughScience && target > 1;

        lblTarget.text = loc.T("LV.LGP.SetMul", target, sciences.CalculateScienceCost(target));
        
        lblPrev.text = target > 1 ?
            loc.T("LV.LGP.PrevCost", target -1, sciences.CalculateScienceCost(target - 1)) :
            loc.T("LV.LGP.NoPrev");

        var max = s.MaxMultiplier;
        lblNext.text = target < max ?
            loc.T("LV.LGP.NextCost", target + 1, sciences.CalculateScienceCost(target + 1)) :
            loc.T("LV.LGP.NoNext");
    }

    private static void SetVisibility(VisualElement element, bool visible)
    {
        element.visible = visible;
        element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
