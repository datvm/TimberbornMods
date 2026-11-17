namespace MechanizedManufactory.Services;

[MultiBind(typeof(ISpecServiceTailRunner))]
public class ToolGroupCopier(BlueprintSourceService bss) : ISpecServiceTailRunner
{
    public const string IAGroupId = "MechanizedManufactory";
    static readonly ImmutableArray<string> DefaultParents = [IAGroupId];

    public void Run(SpecService specService)
    {
        var blueprints = specService._cachedBlueprintsBySpecs[typeof(BlockObjectToolGroupSpec)];

        foreach (var bp in blueprints.Select(b => b.Value).ToArray())
        {
            var org = bp.GetSpec<BlockObjectToolGroupSpec>();
            if (org.Id == IAGroupId) { continue; }

            var copy = new EditableBlueprint(GetIAId(bp.Name));
            copy.Specs.Add(new BlockObjectToolGroupSpec()
            {
                Id = GetIAId(org.Id),
                Order = org.Order,
                NameLocKey = org.NameLocKey,
                Icon = org.Icon,
                FallbackGroup = false,
            });

            var parentSpec = bp.Specs.OfType<ParentToolGroupSpec>().FirstOrDefault();
            var parents = parentSpec is null || parentSpec.ParentIds.IsDefaultOrEmpty
                ? DefaultParents
                : [.. parentSpec.ParentIds.Select(GetIAId)];

            copy.Specs.Add(new ParentToolGroupSpec()
            {
                ParentIds = parents,
            });

            if (bp.Specs.OfType<ToolGroupChildrenSpec>().FirstOrDefault() is { } children)
            {
                copy.Specs.Add(children with
                {
                    ChildrenGroupsIds = MapIds(children.ChildrenGroupsIds),
                    ChildrenToolsTemplateNames = MapIds(children.ChildrenToolsTemplateNames),
                    ChildrenOrderedIds = MapIds(children.ChildrenOrderedIds),
                    ChildrenExplicitOrderedIds = children.ChildrenExplicitOrderedIds.IsDefaultOrEmpty
                        ? []
                        : [.. children.ChildrenExplicitOrderedIds.Select(static q => q with { Id = GetIAId(q.Id) })],
                });
            }

            var copied = copy.ToBlueprint();
            var bssSrc = bss.Get(bp).AddJson("{}", nameof(ToolGroupCopier));
            bss.Add(copied, bssSrc);
            blueprints.Add(new(() => copied));
        }
    }

    public static string GetIAId(string id) => "IA." + id;

    static ImmutableArray<string> MapIds(ImmutableArray<string> ids)
        => ids.IsDefaultOrEmpty ? [] : [.. ids.Select(GetIAId)];

}
