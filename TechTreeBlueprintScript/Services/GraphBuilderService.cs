namespace TechTreeBlueprintScript.Services;

[BindSingleton]
public class GraphBuilderService(
    TemplateProvider templateProvider,
    RecipeProvider recipeProvider
)
{

    public BlueprintDependencyGraph BuildGraph(string factionId)
    {
        var yieldSources = GetYieldSources(factionId).ToArray();
        var buildings = ParseBuildings(factionId, yieldSources).ToList();
        var nodes = buildings.Select(b => new BlueprintDependencyNode(b)).ToList();

        WireDependencies(nodes);
        ReduceTransitiveParents(nodes);

        var roots = nodes
            .Where(n => n.Parents.Count == 0)
            .OrderBy(n => n.Building.Blueprint.GetTemplateName(), StringComparer.Ordinal)
            .ToImmutableArray();

        BlueprintDependencyGraph graph = new(roots);
        SortChildren(graph);
        return graph;
    }

    static void SortChildren(BlueprintDependencyGraph graph) 
        => graph.ScanNodes(n => n.Children.Sort((a, b) => a.Building.Order.CompareTo(b.Building.Order)));

    /// <summary>
    /// Natural resources / plants that yield goods when cut or gathered.
    /// Group is the yielder ResourceGroup (e.g. Cuttable, Tappable), not the planter group.
    /// </summary>
    IEnumerable<YieldSourceInfo> GetYieldSources(string factionId)
    {
        foreach (var bp in templateProvider.TemplatesByFaction[factionId])
        {
            if (bp.TryGetSpec<CuttableSpec>(out var cs))
            {
                yield return new(bp, cs.Yielder.ResourceGroup, cs.Yielder.Yield.Id);
            }

            if (bp.TryGetSpec<GatherableSpec>(out var gs))
            {
                yield return new(bp, gs.Yielder.ResourceGroup, gs.Yielder.Yield.Id);
            }
        }
    }

    IEnumerable<ParsedBuildingInfo> ParseBuildings(string factionId, YieldSourceInfo[] yieldSources)
    {
        foreach (var bp in templateProvider.TemplatesByFaction[factionId])
        {
            var placeable = bp.GetSpec<PlaceableBlockObjectSpec>();
            if (placeable is null || placeable.DevModeTool)
            {
                continue;
            }

            var buildingSpec = bp.GetSpec<BuildingSpec>();
            if (buildingSpec is null) { continue; }

            HashSet<string> required = [.. buildingSpec.BuildingCost.Select(c => c.Id)];
            HashSet<string> produces = [];
            HashSet<string> otherTags = [];
            bool isGatherer = false,
                isManufactory = false,
                isPlanter = false;

            if (placeable.ToolGroupId is { Length: > 0 } toolGroup)
            {
                otherTags.Add($"ToolGroup:{toolGroup}");
            }

            // Lumberjack / scavenge / tapper / etc.: harvest goods from natural resources.
            if (bp.TryGetSpec<YieldRemovingBuildingSpec>(out var yrs))
            {
                isGatherer = true;
                otherTags.Add($"YieldGroup:{yrs.ResourceGroup}");

                foreach (var goodId in yieldSources
                    .Where(y => y.Group == yrs.ResourceGroup)
                    .Select(y => y.GoodId)
                    .Distinct())
                {
                    produces.Add(goodId);
                }
            }

            // Workshops: consume recipe ingredients (+ fuel), produce recipe products.
            if (bp.TryGetSpec<ManufactorySpec>(out var mfg))
            {
                isManufactory = true;

                foreach (var recipeId in mfg.ProductionRecipeIds)
                {
                    if (!recipeProvider.TryGet(recipeId, out var recipe))
                    {
                        throw new InvalidOperationException(
                            $"Building {bp.GetTemplateName()} references unknown recipe '{recipeId}'");
                    }

                    foreach (var ingredient in recipe.Ingredients)
                    {
                        required.Add(ingredient.Id);
                    }

                    if (!string.IsNullOrEmpty(recipe.Fuel))
                    {
                        required.Add(recipe.Fuel);
                    }

                    foreach (var product in recipe.Products)
                    {
                        produces.Add(product.Id);
                    }
                }
            }

            // Foresters / farmers: enable planting a resource group (no direct goods output).
            if (bp.TryGetSpec<PlanterBuildingSpec>(out var planter))
            {
                isPlanter = true;
            }

            yield return new(
                bp,
                placeable.ToolOrder,
                bp.GetTemplateName(),
                required.ToFrozenSet(),
                produces.ToFrozenSet(),
                otherTags.ToFrozenSet(),
                isGatherer,
                isManufactory,
                isPlanter
            );
        }
    }

    /// <summary>
    /// Link A → B when A produces a good that B requires (build cost, recipe ingredient, or fuel).
    /// </summary>
    static void WireDependencies(List<BlueprintDependencyNode> nodes)
    {
        Dictionary<string, List<BlueprintDependencyNode>> producersByGood = [];

        foreach (var node in nodes)
        {
            foreach (var goodId in node.Building.Produces)
            {
                if (!producersByGood.TryGetValue(goodId, out var list))
                {
                    producersByGood[goodId] = list = [];
                }

                list.Add(node);
            }
        }

        foreach (var consumer in nodes)
        {
            foreach (var goodId in consumer.Building.Required)
            {
                if (!producersByGood.TryGetValue(goodId, out var producers))
                {
                    continue;
                }

                foreach (var producer in producers)
                {
                    if (ReferenceEquals(producer, consumer))
                    {
                        continue;
                    }

                    if (consumer.Parents.Contains(producer))
                    {
                        continue;
                    }

                    consumer.Parents.Add(producer);
                    producer.Children.Add(consumer);
                }
            }
        }
    }

    /// <summary>
    /// Drop redundant parents: if both A and B are parents of C, and A is an ancestor of B,
    /// remove A→C (e.g. GearWorkshop keeps LumberMill, not Lumberjack when LumberMill already
    /// requires Lumberjack).
    /// </summary>
    static void ReduceTransitiveParents(List<BlueprintDependencyNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (node.Parents.Count <= 1)
            {
                continue;
            }

            List<BlueprintDependencyNode> redundant = [];
            foreach (var parent in node.Parents)
            {
                foreach (var other in node.Parents)
                {
                    if (ReferenceEquals(parent, other))
                    {
                        continue;
                    }

                    // parent → … → other → node makes parent → node redundant.
                    if (IsAncestor(parent, other))
                    {
                        redundant.Add(parent);
                        break;
                    }
                }
            }

            foreach (var parent in redundant)
            {
                node.Parents.Remove(parent);
                parent.Children.Remove(node);
            }
        }
    }

    /// <summary>True if <paramref name="ancestor"/> can reach <paramref name="node"/> via parent links (upward).</summary>
    static bool IsAncestor(BlueprintDependencyNode ancestor, BlueprintDependencyNode node)
    {
        HashSet<BlueprintDependencyNode> visited = [];
        Stack<BlueprintDependencyNode> stack = [];
        stack.Push(node);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            foreach (var parent in current.Parents)
            {
                if (ReferenceEquals(parent, ancestor))
                {
                    return true;
                }

                if (visited.Add(parent))
                {
                    stack.Push(parent);
                }
            }
        }

        return false;
    }

    record YieldSourceInfo(ScriptBlueprint Blueprint, string Group, string GoodId);

}
