namespace Hospital.Patches;

[HarmonyPatch(typeof(PrefabGroupService))]
public static class PrefabPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(PrefabGroupService.Load))]
    public static void UpdateMedicalPrefabs(PrefabGroupService __instance)
    {
        foreach (var prefab in __instance.AllPrefabs)
        {
            var name = prefab.GetComponent<PrefabSpec>()?.PrefabName;

            if (name is "DoubleMedicalBed.Folktails" or "DoubleMedicalBed.IronTeeth")
            {
                ModifyPrefab(prefab, "DoubleBed",
                350,
                [
                    new() {
                        _goodId = "Log",
                        _amount = 10
                    },
                    new() {
                        _goodId = "Plank",
                        _amount = 8
                    }
                ],
                2, 2, 0.015f);
            }
            else if (name is "Hospital.Folktails" or "Hospital.IronTeeth")
            {
                ModifyPrefab(prefab, "Hospital",
                1500,
                [
                    new() {
                        _goodId = "Log",
                        _amount = 40
                    },
                    new() {
                        _goodId = "Plank",
                        _amount = 20
                    },
                    new() {
                        _goodId = "MetalBlock",
                        _amount = 20
                    }
                ],
                10, 4,
                0.06f,
                new()
                {
                    _supply = "Extract",
                    _capacity = 10,
                    _goodPerHour = 1f,
                });
            }
        }
    }

    static void ModifyPrefab(GameObject obj, string name,
        int scienceCost, GoodAmountSpec[] buildingCost,
        int slots,
        int height,
        float injuryPerHours,
        GoodConsumingBuildingSpec? goodConsuming = null
    )
    {
        obj.AddComponent<HospitalComponentSpec>();

        var prefab = obj.GetComponent<PrefabSpec>();
        prefab._prefabName = name;

        var label = prefab.GetComponentFast<LabeledEntitySpec>();
        var loc = label._displayNameLocKey = "LV.Hospt." + name;
        label._descriptionLocKey = loc + "Desc";
        label._flavorDescriptionLocKey = loc + "Flavor";
        label._imagePath = $"Sprites/Buildings/Wellbeing/{name}/{name}";

        var building = prefab.GetComponentFast<BuildingSpec>();
        building._scienceCost = scienceCost;
        building._buildingCost = buildingCost;

        var transformSlot = prefab.GetComponentFast<TransformSlotInitializerSpec>();
        while (transformSlot._slots.Count < slots)
        {
            transformSlot._slots.Add(transformSlot._slots[0]);
        }

        var enterable = prefab.GetComponentFast<EnterableSpec>();
        enterable._capacityFinished = slots;

        var blockObj = prefab.GetComponentFast<BlockObjectSpec>();
        ResizeBlockObj(blockObj, height);

        var attraction = prefab.GetComponentFast<AttractionSpec>();
        var eff = attraction._effects.First(q => q._needId == "Injury");
        eff._pointsPerHour = injuryPerHours;

        if (goodConsuming is not null)
        {
            var goodConsumingBuilding = obj.AddComponent<GoodConsumingBuildingSpec>();
            goodConsumingBuilding._capacity = goodConsuming._capacity;
            goodConsumingBuilding._goodPerHour = goodConsuming._goodPerHour;
            goodConsumingBuilding._supply = goodConsuming._supply;

            obj.AddComponent<GoodConsumingAttractionSpec>();
        }
    }

    static void ResizeBlockObj(BlockObjectSpec blockObj, int height)
    {
        var spec = blockObj._blocksSpec;
        spec._size = spec._size with { z = height, };

        var baseBlock = spec._blockSpecs[0];
        List<BlockSpec> blocks = [];
        for (int i = 0; i < height; i++)
        {
            var occupation = baseBlock._occupations;
            if (i < height - 1)
            {
                occupation |= BlockOccupations.Top;
            }

            blocks.Add(baseBlock with
            {
                _matterBelow = MatterBelow.Any,
                _occupations = occupation,
            });
        }
        spec._blockSpecs = [.. blocks];

        blockObj._blocksSpec = spec;
    }


}
