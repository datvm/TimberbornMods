namespace ModdableAlternativeBuildingStyle.Components;

[AddTemplateModule2(typeof(Floodgate))]
public class FloodgateStyleListener(MaterialHeightCutoffSetter cutoff, ISpecService specs) : BaseComponent, IAlternativeBuildingStyleListener
{
    // Graft renames "#Name" to this before the listener runs.
    const string RenamedPrefix = "#Alternative-";

    public void OnAlternativeStyleChanged(AlternativeBuildingStyleComponent building, string? style)
    {
        var floodgate = GetComponent<Floodgate>();
        var animation = GetComponent<FloodgateAnimationController>();
        if (!floodgate || !animation)
        {
            return;
        }

        var gate = VisibleGate(building, style) ?? BuildingGate(animation);
        if (!gate)
        {
            return;
        }

        animation.Gate = gate;
        var height = floodgate.IsInputOn ? floodgate.AutomationHeight : floodgate.Height;
        animation.MoveGateInstantly(height);

        var renderer = gate.GetComponentInChildren<MeshRenderer>();
        var center = GetComponent<BlockObjectCenter>();
        if (!renderer || !center)
        {
            return;
        }

        cutoff.SetCutoff(renderer.material, center.WorldCenterGrounded.y);
    }

    Transform? VisibleGate(AlternativeBuildingStyleComponent building, string? style)
    {
        var gateName = StyleGateName(style);
        var instance = building.State.StyleInstance;
        if (gateName is null || !instance)
        {
            return null;
        }

        var gate = FindNamed(instance.transform, gateName);
        if (!gate)
        {
            Debug.LogError($"Floodgate style '{style}' has no gate named '{gateName}' on {instance.name}.");
        }

        return gate;
    }

    string? StyleGateName(string? style)
    {
        if (style is null)
        {
            return null;
        }

        var styles = GetComponent<AlternativeBuildingStyleSpec>();
        if (styles is null)
        {
            return null;
        }

        string? path = null;
        foreach (var entry in styles.Styles)
        {
            if (entry.Name == style)
            {
                path = entry.BlueprintPath;
                break;
            }
        }

        if (path is null)
        {
            return null;
        }

        var blueprint = specs.GetBlueprint(path);
        if (!blueprint.HasSpec<FloodgateAnimationStyleSpec>())
        {
            return null;
        }

        var gateName = blueprint.GetSpec<FloodgateAnimationStyleSpec>().GateName;
        if (string.IsNullOrEmpty(gateName))
        {
            return null;
        }

        return gateName;
    }

    Transform? BuildingGate(FloodgateAnimationController animation)
    {
        var spec = GetComponent<FloodgateAnimationControllerSpec>();
        if (spec is null || string.IsNullOrEmpty(spec.GateName))
        {
            return animation.Gate;
        }

        return GameObject.FindChildTransform(spec.GateName);
    }

    static Transform? FindNamed(Transform root, string gateName)
    {
        var renamed = gateName.StartsWith('#') ? RenamedPrefix + gateName[1..] : null;
        foreach (var child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == gateName || (renamed is not null && child.name == renamed))
            {
                return child;
            }
        }

        return null;
    }
}
