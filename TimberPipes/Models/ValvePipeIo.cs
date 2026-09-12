namespace TimberPipes.Models;

public static class ValvePipeIo
{
    public static bool IsBuildingCandidate(
        bool finished,
        bool hasActiveInventory,
        bool isTransportPipe,
        bool isTank = false)
        => finished && hasActiveInventory && !isTransportPipe && !isTank;

    public static bool HasActiveInventory(int enabledInventoryCount)
        => enabledInventoryCount > 0;

    public static bool CanInlet(bool enabled, bool paused, bool contaminated, float volume, string? networkGoodId)
    {
        if (!enabled || paused || contaminated)
        {
            return false;
        }

        if (networkGoodId is null || networkGoodId.Length == 0)
        {
            return false;
        }

        return volume >= PipeFluids.PacketVolume;
    }

    public static bool CanOutlet(bool enabled, bool paused, bool contaminated, float freeSpace, string? selectedGoodId)
    {
        if (!enabled || paused || contaminated)
        {
            return false;
        }

        if (selectedGoodId is null || selectedGoodId.Length == 0)
        {
            return false;
        }

        return HasExtractSpace(freeSpace);
    }

    public static bool HasExtractSpace(float freeSpace)
        => freeSpace > PipeFluids.MoveEpsilon;

    public static bool CanPourExtractBuffer(bool paused, float buffer, float freeSpace)
        => !paused && buffer > PipeFluids.MoveEpsilon && HasExtractSpace(freeSpace);

    public static float ExtractPourAmount(float buffer, float freeSpace)
    {
        if (buffer <= 0f || freeSpace <= 0f)
        {
            return 0f;
        }

        return Math.Min(buffer, freeSpace);
    }

    public static bool CanGiveToBuilding(bool takes, bool hasUnreservedCapacity)
        => takes && hasUnreservedCapacity;

    public static bool CanTakeFromBuilding(int takeableAmount)
        => takeableAmount > 0;

    public static bool HasLiquidInput(IEnumerable<string> inputGoods, HashSet<string> liquids)
    {
        foreach (var id in inputGoods)
        {
            if (liquids.Contains(id))
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasKnownExtractLiquid(
        IEnumerable<string> outputGoods,
        IEnumerable<string> takeableStock,
        HashSet<string> liquids)
        => HasLiquidInput(outputGoods, liquids) || HasLiquidInput(takeableStock, liquids);

    public static bool TargetStillValid(bool buildingPresent, bool finished, int enabledInventoryCount)
        => buildingPresent && finished && HasActiveInventory(enabledInventoryCount);

    public static HashSet<string> LiquidIds(IGoodService goods)
    {
        if (liquidIds is { } cached)
        {
            return cached;
        }

        HashSet<string> ids = [];
        foreach (var id in goods.GetGoodsForType(PipeFluids.LiquidGoodType))
        {
            if (goods.HasGood(id))
            {
                ids.Add(id);
            }
        }

        liquidIds = ids;
        return ids;
    }

    static HashSet<string>? liquidIds;

    public static List<string> KnownExtractLiquids(
        IEnumerable<string> outputGoods,
        IEnumerable<string> takeableStock,
        HashSet<string> liquids)
    {
        HashSet<string> ids = [];
        foreach (var id in outputGoods)
        {
            if (liquids.Contains(id))
            {
                ids.Add(id);
            }
        }

        foreach (var id in takeableStock)
        {
            if (liquids.Contains(id))
            {
                ids.Add(id);
            }
        }

        return [.. ids];
    }

    public static List<string> ExtractDropdownGoods(
        List<string> known,
        IEnumerable<string> allLiquids,
        string? storedGoodId = null)
    {
        List<string> ids = known.Count > 0 ? [.. known] : [.. allLiquids];
        if (storedGoodId is { Length: > 0 } && !ids.Contains(storedGoodId))
        {
            ids.Add(storedGoodId);
        }

        return ids;
    }

    public static string? DefaultExtractGood(string? storedGoodId, IReadOnlyList<string> available)
    {
        if (storedGoodId is { Length: > 0 })
        {
            return storedGoodId;
        }

        return available.Count > 0 ? available[0] : null;
    }

    public static bool ShouldInlet(
        bool enabled,
        bool paused,
        bool contaminated,
        float volume,
        string? networkGoodId,
        bool takes,
        bool hasUnreservedCapacity)
        => CanInlet(enabled, paused, contaminated, volume, networkGoodId)
            && CanGiveToBuilding(takes, hasUnreservedCapacity);

    public static bool ShouldOutlet(
        bool enabled,
        bool paused,
        bool contaminated,
        float freeSpace,
        string? selectedGoodId,
        int takeableAmount,
        float buffer = 0f)
        => CanOutlet(enabled, paused, contaminated, freeSpace, selectedGoodId)
            && buffer <= PipeFluids.MoveEpsilon
            && CanTakeFromBuilding(takeableAmount);
}

public readonly record struct ValveIoTarget(BlockObject Building, Inventories Inventories);
