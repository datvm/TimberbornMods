namespace MechanizedManufactory.Services;

[MultiBind(typeof(ITemplateCollectionServiceTailRunner))]
public class ManufactoryCopier(
    BlueprintSourceService bss,
    ILoc loc,
    LocalizationEntryAdder locAdder,
    MSettings s
) : ITemplateCollectionServiceTailRunner
{
    public int Order => 1_000;

    public void Run(TemplateCollectionService service)
    {
        List<Blueprint> copies = [];

        foreach (var t in service.AllTemplates)
        {
            if (t.HasSpec<ManufactorySpec>()
                && t.HasSpec<PlaceableBlockObjectSpec>()
                && t.GetSpec<WorkplaceSpec>() is { MaxWorkers: > 0 })
            {
                if (t.GetSpec<MechanicalNodeSpec>() is { PowerOutput: > 0 })
                {
                    continue; // Do not copy power generators
                }

                var copy = MakeIndustrializedCopy(t);
                var src = bss.Get(t).AddJson("{}", nameof(ManufactoryCopier));
                bss.Add(copy, src);
                copies.Add(copy);
            }
        }

        if (copies.Count == 0) { return; }

        service.AllTemplates = [.. service.AllTemplates, .. copies];
        locAdder.Commit();
    }

    Blueprint MakeIndustrializedCopy(Blueprint t)
    {
        var bp = new EditableBlueprint(t)
        {
            Name = ToolGroupCopier.GetIAId(t.Name),
        };
        var specs = bp.Specs;

        int maxWorkers = 0;
        MechanicalNodeSpec? mechSpec = null;
        var hasTransput = false;
        var hasConnector = false;
        var hasMechanicalBuilding = false;
        BlockObjectSpec bos = null!;

        for (int i = specs.Count - 1; i >= 0; i--)
        {
            var spec = specs[i];

            switch (spec)
            {
                case WorkplaceSpec wp:
                    maxWorkers = wp.MaxWorkers;
                    specs[i] = new WorkplaceSpec()
                    {
                        MaxWorkers = 0,
                        DefaultWorkers = 0,
                        DefaultWorkerType = "Beaver",
                        DisallowOtherWorkerTypes = true,
                        WorkerTypeUnlockCosts = [],
                    };
                    break;
                case PlaceableBlockObjectSpec p:
                    specs[i] = p with
                    {
                        ToolGroupId = ToolGroupCopier.GetIAId(p.ToolGroupId),
                    };

                    break;
                case ParentToolGroupSpec p:
                    specs[i] = p with
                    {
                        ParentIds = [.. p.ParentIds.Select(ToolGroupCopier.GetIAId)],
                    };

                    break;
                case MechanicalNodeSpec m:
                    mechSpec = m;
                    specs.RemoveAt(i);
                    break;
                case TransputProviderSpec:
                    hasTransput = true;
                    break;
                case MechanicalConnectorTargetSpec:
                    hasConnector = true;
                    break;
                case MechanicalBuildingSpec:
                    hasMechanicalBuilding = true;
                    break;
                case BlockObjectSpec bo:
                    bos = bo;
                    break;
                case TemplateSpec template:
                    specs[i] = template with
                    {
                        TemplateName = ToolGroupCopier.GetIAId(template.TemplateName),
                        BackwardCompatibleTemplateNames = [],
                    };
                    break;
                case LabeledEntitySpec les:
                    var key = les.DisplayNameLocKey;
                    var newName = loc.T("LV.MM.BuildingName", loc.T(key));

                    var newKey = GetNewLocKey(key);
                    locAdder.Set(newKey, newName);
                    specs[i] = les with
                    {
                        DisplayNameLocKey = newKey,
                    };

                    break;
                case ProductionResetterSpec:
                    // Treats an empty workplace as abandoned and wipes craft progress.
                    specs.RemoveAt(i);
                    break;
            }
        }

        specs.Add(new MechanizedManufactorySpec()
        {
            Workers = maxWorkers,
        });

        if (mechSpec is null)
        {
            specs.Add(new MechanicalNodeSpec()
            {
                PowerInput = GetPower(maxWorkers),
            });
        }
        else
        {
            specs.Add(mechSpec with
            {
                PowerInput = GetPower(maxWorkers, mechSpec.PowerInput),
            });
        }

        if (!hasMechanicalBuilding)
        {
            specs.Add(new MechanicalBuildingSpec());
        }

        if (!hasTransput)
        {
            specs.Add(GenerateTransputs(bos));
        }

        if (!hasConnector)
        {
            specs.Add(new MechanicalConnectorTargetSpec());
        }

        return bp.ToBlueprint();
    }

    public static int GetPower(int workers, int original = 0) => original + workers * MSettings.PowerPerWorkerValue;

    // Horizontal openings only. Top and Bottom stay closed so shafts connect on the sides.
    static TransputProviderSpec GenerateTransputs(BlockObjectSpec bos)
    {
        var occupied = bos.GetBlocks().GetOccupiedCoordinates().ToHashSet();
        List<TransputSpec> transputs = [];

        foreach (var coords in occupied)
        {
            var directions = Directions3D.None;

            foreach (var delta in Deltas.Neighbors4Vector3Int)
            {
                if (occupied.Contains(coords + delta)) { continue; }

                directions |= Direction3DExtensions.FromOffset(delta).ToDirections3D();
            }

            if (directions == Directions3D.None) { continue; }

            transputs.Add(new()
            {
                Coordinates = coords,
                Directions = directions,
            });
        }

        return new()
        {
            Transputs = [.. transputs],
        };
    }

    static string GetNewLocKey(string key) => "LV.MM." + key;

}
