namespace ModdableAlternativeBuildingStyle.Components;

[AddTemplateModule2(typeof(FireworkLauncher))]
public class FireworkLauncherStyleListener(ISpecService specs) : BaseComponent, IAlternativeBuildingStyleListener
{
    // Graft renames "#Name" to this before the listener runs.
    const string RenamedPrefix = "#Alternative-";

    readonly record struct CannonNames(string Turret, string Barrel);

    readonly record struct CannonParts(Transform Turret, Transform Barrel);

    public void OnAlternativeStyleChanged(AlternativeBuildingStyleComponent building, string? style)
    {
        var model = GetComponent<FireworkLauncherModel>();
        if (!model)
        {
            return;
        }

        var cannon = VisibleCannon(building, style) ?? BuildingCannon(model);
        if (cannon is null)
        {
            return;
        }

        // Awake caches these, and later aim changes write localRotation on the same fields.
        model._turret = cannon.Value.Turret;
        model._barrel = cannon.Value.Barrel;
        model.UpdateModel();
    }

    CannonParts? VisibleCannon(AlternativeBuildingStyleComponent building, string? style)
    {
        var names = StyleNames(style);
        var instance = building.State.StyleInstance;
        if (names is null || !instance)
        {
            return null;
        }

        var turret = FindNamed(instance.transform, names.Value.Turret);
        var barrel = FindNamed(instance.transform, names.Value.Barrel);
        if (!turret || !barrel)
        {
            Debug.LogError(
                $"Firework launcher style '{style}' is missing '{names.Value.Turret}' or '{names.Value.Barrel}' on {instance.name}.");
            return null;
        }

        return new CannonParts(turret, barrel);
    }

    CannonNames? StyleNames(string? style)
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
        if (!blueprint.HasSpec<FireworkLauncherAnimationStyleSpec>())
        {
            return null;
        }

        var cannon = blueprint.GetSpec<FireworkLauncherAnimationStyleSpec>();
        if (string.IsNullOrEmpty(cannon.Turret) || string.IsNullOrEmpty(cannon.Barrel))
        {
            return null;
        }

        return new CannonNames(cannon.Turret, cannon.Barrel);
    }

    CannonParts? BuildingCannon(FireworkLauncherModel model)
    {
        var spec = GetComponent<FireworkLauncherSpec>();
        if (spec is null || string.IsNullOrEmpty(spec.Turret) || string.IsNullOrEmpty(spec.Barrel))
        {
            if (!model._turret || !model._barrel)
            {
                return null;
            }

            return new CannonParts(model._turret, model._barrel);
        }

        return new CannonParts(
            GameObject.FindChildTransform(spec.Turret),
            GameObject.FindChildTransform(spec.Barrel));
    }

    static Transform? FindNamed(Transform root, string partName)
    {
        var renamed = partName.StartsWith('#') ? RenamedPrefix + partName[1..] : null;
        foreach (var child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == partName || (renamed is not null && child.name == renamed))
            {
                return child;
            }
        }

        return null;
    }
}
