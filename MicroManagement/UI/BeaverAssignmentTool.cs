

namespace MicroManagement.UI;

public class BeaverAssignmentTool(InputService input, Highlighter highlighter, SelectableObjectRaycaster selectableObjectRaycaster) : IInputProcessor
{
    static readonly Color PickColor = Color.green;

    PickingAssignment? picking;

    public void PickAssignment(in PickingAssignment picking)
    {
        this.picking = picking;
        input.AddInputProcessor(this);
    }

    public void Unassign(Citizen citizen, CitizenAssignmentType type)
    {
        switch (type)
        {
            case CitizenAssignmentType.Dwelling:
                var dweller = citizen.GetComponentFast<Dweller>();
                if (dweller)
                {
                    KickDweller(dweller);
                }

                break;
            case CitizenAssignmentType.Workplace:
                var worker = citizen.GetComponentFast<Worker>();
                if (worker)
                {
                    KickWorker(worker);
                }

                break;
        }
    }

    public bool ProcessInput()
    {
        if (picking is null) // Should not happen but who knows
        {
            UnregisterInput();
            return false;
        }

        if (input.Cancel)
        {
            UnregisterInput();
            return true;
        }

        highlighter.UnhighlightAllPrimary();

        var obj = TryGetSelectableObject();
        if (!obj) { return false; }

        highlighter.HighlightPrimary(obj, PickColor);
        if (input.MainMouseButtonUp)
        {
            ProcessPick(obj);
            return true;
        }

        return false;
    }

    void ProcessPick(BaseComponent obj)
    {
        if (picking is null)
        {
            UnregisterInput();
            return;
        }

        var (citizen, type, callback) = picking.Value;
        UnregisterInput();

        var targetDistrict = FindBuildingDistrict(obj);
        if (targetDistrict is null)
        {
            callback("LV.MM.ErrNoDistrict");
            return;
        }

        var citizenDistrict = FindCitizenDistrict(citizen);
        // Do not check null here, a citizen may be stranded

        if (type == CitizenAssignmentType.District || citizenDistrict != targetDistrict)
        {
            MoveToDistrict(citizen, targetDistrict);
        }

        switch (type)
        {
            case CitizenAssignmentType.Dwelling:
                MoveToHouse(citizen, obj);
                break;
            case CitizenAssignmentType.Workplace:
                MoveToWorkplace(citizen, obj);
                break;
        }
    }

    void MoveToHouse(Citizen citizen, BaseComponent comp)
    {
        var dwelling = comp as Dwelling ?? throw new InvalidDataException($"{nameof(comp)} is not {nameof(Dwelling)}");
        var dweller = citizen.GetComponentFast<Dweller>();

        if (!dwelling.HasFreeSlots)
        {
            KickARandomDweller(dwelling);
        }

        dwelling.AssignDweller(dweller);
    }

    void KickARandomDweller(Dwelling dwelling)
    {
        var count = dwelling.NumberOfDwellers;
        if (count == 0) { return; }

        var index = UnityEngine.Random.RandomRangeInt(0, count - 1);
        var adultCount = dwelling.NumberOfAdultDwellers;
        var dweller = index < adultCount ? 
            dwelling.AdultDwellers.Skip(index).First() : 
            dwelling.ChildDwellers.Skip(index - adultCount).First();

        dwelling.UnassignDweller(dweller);
    }

    public void KickDweller(Dweller dweller)
    {
        var dwelling = dweller.Home;
        if (!dwelling) { return; }

        dwelling.UnassignDweller(dweller);
    }

    void MoveToWorkplace(Citizen citizen, BaseComponent comp)
    {
        var workplace = comp as Workplace ?? throw new InvalidDataException($"{nameof(comp)} is not {nameof(Workplace)}");
        if (workplace.DesiredWorkers == 0) { return; }

        var worker = citizen.GetComponentFast<Worker>();

        if (!workplace.Understaffed)
        {
            KickARandomWorker(workplace);
        }

        workplace.AssignWorker(worker);
    }

    void KickARandomWorker(Workplace workplace)
    {
        var count = workplace.NumberOfAssignedWorkers;
        if (count == 0) { return; }

        var index = UnityEngine.Random.RandomRangeInt(0, count - 1);
        var worker = workplace.AssignedWorkers.Skip(index).First();
        workplace.UnassignWorker(worker);
    }

    public void KickWorker(Worker worker)
    {
        var workplace = worker.Workplace;
        if (!workplace) { return; }
        workplace.UnassignWorker(worker);
    }

    void MoveToDistrict(Citizen citizen, DistrictCenter district)
    {
        citizen.AssignDistrict(district);

        var blockObj = district.GetComponentFast<BlockObject>();
        if (!blockObj || blockObj.PositionedEntrance is null)
        {
            throw new InvalidDataException("DistrictCenter has no entrance");
        }

        var coord = blockObj.PositionedEntrance.DoorstepCoordinates;

        var walker = citizen.GetComponentFast<Walker>();
        walker.StopMoving();

        citizen.TransformFast.position = CoordinateSystem.GridToWorld(coord);
    }

    DistrictCenter? FindBuildingDistrict(BaseComponent comp)
    {
        var districtBuilding = comp.GetComponentFast<DistrictBuilding>();
        return districtBuilding ? districtBuilding.District : null;
    }

    DistrictCenter? FindCitizenDistrict(Citizen citizen)
    {
        return citizen ? citizen.AssignedDistrict : null;
    }

    BaseComponent? TryGetSelectableObject()
    {
        var type = picking!.Value.Type;

        if (!selectableObjectRaycaster.TryHitSelectableObject(out var obj)) { return null; }

        var blockObj = obj.GetComponentFast<BlockObject>();
        if (!blockObj || !blockObj.IsFinished) { return null; }

        BaseComponent? result = type switch
        {
            CitizenAssignmentType.Dwelling => obj.GetComponentFast<Dwelling>(),
            CitizenAssignmentType.Workplace => obj.GetComponentFast<Workplace>(),
            CitizenAssignmentType.District => obj.GetComponentFast<DistrictCenter>(),
            _ => null,
        };

        if (!result) { return null; }

        if (type == CitizenAssignmentType.Workplace && result is Workplace workplace)
        {
            var worker = picking.Value.Citizen.GetComponentFast<Worker>();
            if (worker.WorkerType != workplace._workplaceWorkerType.WorkerType) { return null; }
        }

        var pausable = result.GetComponentFast<PausableBuilding>();
        if (pausable && pausable.Paused) { return null; }

        return result;
    }

    void UnregisterInput()
    {
        highlighter.UnhighlightAllPrimary();
        picking = null;
        input.RemoveInputProcessor(this);
    }

}

public readonly record struct PickingAssignment(Citizen Citizen, CitizenAssignmentType Type, Action<string?> OnDoneCallback);

public enum CitizenAssignmentType
{
    Dwelling,
    Workplace,
    District,
}