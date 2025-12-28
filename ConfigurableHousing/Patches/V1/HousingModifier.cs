namespace ConfigurableHousing.Patches.V1;

public class HousingModifier(MSettings settings) : ITemplateModifier, ILoadableSingleton
{
    bool addProcreation;
    bool removeProcreation;

    public void Load()
    {
        var addOthers = settings.AddOtherFaction.Value;

        addProcreation = addOthers && settings.AddProcreation.Value;
        removeProcreation = addOthers && settings.RemoveProcreation.Value;
    }

    public EditableBlueprint? Modify(EditableBlueprint template, TemplateSpec originalTemplateSpec, Blueprint original)
    {
        var hasProcreation = false;

        for (int i = 0; i < template.Specs.Count; i++)
        {
            var spec = template.Specs[i];

            if (spec is DwellingSpec d)
            {
                template.Specs[i] = TransformDwellingSpec(d);
            }
            else if (spec is ProcreationHouseSpec)
            {
                if (removeProcreation)
                {
                    template.Specs.RemoveAt(i);
                    i--;
                }
                else
                {
                    hasProcreation = true;
                }
            }
        }

        if (addProcreation && !hasProcreation)
        {
            template.Specs.Add(new ProcreationHouseSpec());
        }

        if (settings.MoveEntranceFloor.Value)
        {
            MoveEntranceToFloor();
        }

        return template;

        DwellingSpec TransformDwellingSpec(DwellingSpec dwellingSpec)
        {
            var effects = new ContinuousEffectSpec[dwellingSpec.SleepEffects.Length];
            for (int i = 0; i < effects.Length; i++)
            {
                var curr = dwellingSpec.SleepEffects[i];
                var points = curr.PointsPerHour * (curr.NeedId switch
                {
                    "Sleep" => settings.SleepSatisfactionMultiplier.Value,
                    "Shelter" => settings.ShelterSatisfactionMultiplier.Value,
                    _ => 1f,
                });

                effects[i] = dwellingSpec.SleepEffects[i] with
                {
                    PointsPerHour = points,
                };
            }

            return dwellingSpec with
            {
                MaxBeavers = Math.Max(1, Mathf.FloorToInt(dwellingSpec.MaxBeavers * settings.MaxBeaverMultiplier.Value) + settings.MaxBeaverAdd.Value),
                SleepEffects = [.. effects],
            };
        }

        void MoveEntranceToFloor()
        {
            var (bosIndex, bos) = FindSpecIndex<BlockObjectSpec>(template.Specs);
            if (bos is null) { return; }

            var (navMeshIndex, navMesh) = FindSpecIndex<BlockObjectNavMeshSettingsSpec>(template.Specs);
            if (navMesh is null) { return; }

            var (accSpecIndex, accSpec) = FindSpecIndex<BuildingAccessibleSpec>(template.Specs);

            var entrance = bos.Entrance;
            if (!entrance.HasEntrance)
            {
                Debug.LogWarning($"ConfigurableHousing: Dwelling {template.Name} has no entrance");
            }

            var entranceZ = entrance.Coordinates.z;
            if (entranceZ == 0) { return; } // Already on floor 0

            template.Specs[bosIndex] = bos with
            {
                Entrance = entrance with
                {
                    Coordinates = entrance.Coordinates with { z = 0 },
                },
            };

            template.Specs[navMeshIndex] = navMesh with
            {
                EdgeGroups = [.. navMesh.EdgeGroups
                    .Select(q => q with
                    {
                        AddedEdges = [.. q.AddedEdges
                            .Select(e => e with
                            {
                                Start = e.Start with { z = e.Start.z - entranceZ },
                                End = e.End with { z = e.End.z - entranceZ },
                            })],
                    })],
            };

            if (accSpec is not null)
            {
                template.Specs[accSpecIndex] = accSpec with
                {
                    LocalAccess = accSpec.LocalAccess with
                    {
                        y = accSpec.LocalAccess.y - entranceZ,
                    },
                };
            }
        }
    }

    static (int, T?) FindSpecIndex<T>(List<ComponentSpec> items) where T : ComponentSpec
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] is T t)
            {
                return (i, t);
            }
        }

        return (-1, null);
    }

    public bool ShouldModify(string blueprintName, string templateName, TemplateSpec originalTemplateSpec)
        => originalTemplateSpec.HasSpec<DwellingSpec>();
}
