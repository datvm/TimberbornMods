namespace ModdableTimberborn.DependencyInjection;

sealed class TemplateModifierTailRunner(
    IEnumerable<ITemplateModifier> modifiers,
    BlueprintSourceService blueprintSourceService
) : ITemplateCollectionServiceTailRunner
{
    static readonly string TypeName = typeof(TemplateModifierTailRunner).FullName;
    readonly ImmutableArray<ITemplateModifier> modifiers = [.. modifiers.OrderBy(q => q.Order)];

    public void Run(TemplateCollectionService templateCollectionService)
    {
        if (modifiers.Length == 0) { return; }

        var list = templateCollectionService.AllTemplates.ToArray();
        var hasAnyChange = false;
        for (int i = 0; i < list.Length; i++)
        {
            var original = list[i];
            var templateSpec = original.GetSpec<TemplateSpec>();
            if (templateSpec is null) { continue; }

            EditableBlueprint? curr = null;
            var changed = false;
            var blueprintName = original.Name;
            var templateName = templateSpec.TemplateName;
            foreach (var m in modifiers)
            {
                if (!m.ShouldModify(blueprintName, templateName, templateSpec)) { continue; }

                curr ??= new(original);
                var modified = m.Modify(curr, templateSpec, original);
                if (modified is null) { continue; }

                changed = true;
                ModdableTimberbornUtils.LogVerbose(() => $"Modified template '{templateName}' ('{blueprintName}') using '{m.GetType().FullName}'");
                curr = modified;
            }

            if (changed)
            {
                var bp = list[i] = curr!;

                var src = blueprintSourceService.Get(original).AddJson("{}", TypeName);
                blueprintSourceService.Add(bp, src);

                hasAnyChange = true;
            }
        }

        if (!hasAnyChange) { return; }
        templateCollectionService.AllTemplates = [.. list];
    }

}
