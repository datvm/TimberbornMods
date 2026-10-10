namespace BeaverController.Services;

[BindSingleton]
public class CharacterAssignmentTool(
    InputService input,
    Highlighter highlighter,
    SelectableObjectRaycaster selectableObjectRaycaster,
    BuildingUseService uses
) : IInputProcessor
{
    static readonly Color PickColor = Color.green;
    static readonly Color WarningColor = Color.orange;

    AssignmentRequest? request;
    bool registered;

    public void Pick(CharacterControllerComponent character, AssignmentKind kind, Action<string?> onDone)
    {
        request = new(character, kind, onDone);
        if (registered) { return; }

        input.AddInputProcessor(this);
        registered = true;
    }

    public void Cancel()
    {
        if (!registered) { return; }
        Unregister();
    }

    public void Unassign(CharacterControllerComponent character, AssignmentKind kind)
    {
        switch (kind)
        {
            case AssignmentKind.Dwelling:
                var dweller = character.GetComponent<Dweller>();
                if (dweller && dweller.HasHome)
                {
                    dweller.Home.UnassignDweller(dweller);
                }

                break;
            case AssignmentKind.Workplace:
                var worker = character.GetComponent<Worker>();
                if (worker && worker.Workplace)
                {
                    worker.Workplace.UnassignWorker(worker);
                }

                break;
        }
    }

    public bool ProcessInput()
    {
        if (request is null)
        {
            Unregister();
            return false;
        }

        if (input.Cancel)
        {
            Finish(null);
            return true;
        }

        if (input.MouseOverUI)
        {
            highlighter.UnhighlightAllPrimary();
            return false;
        }

        highlighter.UnhighlightAllPrimary();
        var target = TryGetTarget();
        if (target is not BaseComponent hit) { return false; }

        highlighter.HighlightPrimary(hit, ShouldWarn(hit) ? WarningColor : PickColor);
        if (!input.MainMouseButtonDown) { return false; }

        Finish(Apply(hit));
        return true;
    }

    string? Apply(BaseComponent target)
    {
        if (request is null) { return null; }

        var (character, kind, _) = request.Value;
        if (kind == AssignmentKind.Use)
        {
            return StartUse(character, target);
        }

        var district = target.GetComponent<DistrictBuilding>();
        if (!district || !district.District)
        {
            return "LV.BC.ErrNoDistrict";
        }

        var citizen = character.GetComponent<Citizen>();
        if (citizen && (kind == AssignmentKind.District || citizen.AssignedDistrict != district.District))
        {
            var error = AssignToDistrict(character, district.District);
            if (error is not null || kind == AssignmentKind.District)
            {
                return error;
            }
        }

        return kind switch
        {
            AssignmentKind.Dwelling => MoveToHouse(character, target),
            AssignmentKind.Workplace => MoveToWorkplace(character, target),
            _ => null,
        };
    }

    string? StartUse(CharacterControllerComponent character, BaseComponent target)
    {
        if (!uses.TryClaim(character, target, out var use) || !character.CommandUse(use))
        {
            return "LV.BC.ErrNoUse";
        }

        return null;
    }

    static string? AssignToDistrict(CharacterControllerComponent character, DistrictCenter district)
    {
        var citizen = character.GetComponent<Citizen>();
        if (!citizen) { return "LV.BC.ErrNoDistrict"; }

        citizen.AssignDistrict(district);
        return null;
    }

    static string? MoveToHouse(CharacterControllerComponent character, BaseComponent target)
    {
        var dwelling = target.GetComponent<Dwelling>();
        var dweller = character.GetComponent<Dweller>();
        if (!dwelling || !dweller) { return "LV.BC.ErrNoHouse"; }
        if (dweller.Home == dwelling) { return null; }

        if (!dwelling.HasFreeSlots && !KickDweller(dwelling))
        {
            return "LV.BC.ErrNoHouse";
        }

        if (!dwelling.HasFreeSlots) { return "LV.BC.ErrNoHouse"; }

        dwelling.AssignDweller(dweller);
        return null;
    }

    static string? MoveToWorkplace(CharacterControllerComponent character, BaseComponent target)
    {
        var workplace = target.GetComponent<Workplace>();
        var worker = character.GetComponent<Worker>();
        if (!workplace || !worker) { return "LV.BC.ErrNoWorkplace"; }
        if (workplace.DesiredWorkers == 0) { return "LV.BC.ErrNoWorkplace"; }
        if (worker.Workplace == workplace) { return null; }

        var workplaceType = workplace.GetComponent<WorkplaceWorkerType>();
        if (!workplaceType || workplaceType.WorkerType != worker.WorkerType)
        {
            return "LV.BC.ErrWrongWorker";
        }

        var blockable = workplace.GetComponent<BlockableObject>();
        if (blockable && !blockable.IsUnblocked) { return "LV.BC.ErrNoWorkplace"; }

        if (workplace.NumberOfAssignedWorkers >= workplace.DesiredWorkers && !KickWorker(workplace))
        {
            return "LV.BC.ErrNoWorkplace";
        }

        if (!workplace.Understaffed) { return "LV.BC.ErrNoWorkplace"; }

        workplace.AssignWorker(worker);
        return null;
    }

    static bool KickDweller(Dwelling dwelling)
    {
        var dweller = dwelling.AdultDwellers.FirstOrDefault() ?? dwelling.ChildDwellers.FirstOrDefault();
        if (dweller is not Dweller kicked) { return false; }

        dwelling.UnassignDweller(kicked);
        return true;
    }

    static bool KickWorker(Workplace workplace)
    {
        if (workplace.NumberOfAssignedWorkers == 0) { return false; }

        workplace.UnassignWorker(workplace.AssignedWorkers[0]);
        return true;
    }

    bool ShouldWarn(BaseComponent component)
    {
        if (request is null) { return false; }
        if (request.Value.Kind == AssignmentKind.Use)
        {
            return uses.IsFull(component);
        }

        return component switch
        {
            Dwelling dwelling => !dwelling.HasFreeSlots,
            Workplace workplace => !workplace.Understaffed,
            _ => false,
        };
    }

    BaseComponent? TryGetTarget()
    {
        if (request is null) { return null; }
        if (!selectableObjectRaycaster.TryHitSelectableObject(out var hit)) { return null; }

        var block = hit.GetComponent<BlockObject>();
        if (!block || !block.IsFinished) { return null; }

        var kind = request.Value.Kind;
        BaseComponent? result = kind switch
        {
            AssignmentKind.Dwelling => hit.GetComponent<Dwelling>(),
            AssignmentKind.Workplace => hit.GetComponent<Workplace>(),
            AssignmentKind.District => hit.GetComponent<DistrictCenter>(),
            AssignmentKind.Use => uses.IsCandidate(request.Value.Character, hit) ? hit : null,
            _ => null,
        };

        if (result is not BaseComponent picked) { return null; }

        if (kind == AssignmentKind.Workplace && picked is Workplace workplace)
        {
            var worker = request.Value.Character.GetComponent<Worker>();
            var workplaceType = workplace.GetComponent<WorkplaceWorkerType>();
            if (!worker || !workplaceType || worker.WorkerType != workplaceType.WorkerType)
            {
                return null;
            }
        }

        var pausable = picked.GetComponent<PausableBuilding>();
        if (pausable && pausable.Paused) { return null; }

        return picked;
    }

    void Finish(string? error)
    {
        var done = request?.OnDone;
        Unregister();
        done?.Invoke(error);
    }

    void Unregister()
    {
        highlighter.UnhighlightAllPrimary();
        request = null;
        if (!registered) { return; }

        input.RemoveInputProcessor(this);
        registered = false;
    }
}

public enum AssignmentKind
{
    Dwelling,
    Workplace,
    District,
    Use,
}

readonly record struct AssignmentRequest(CharacterControllerComponent Character, AssignmentKind Kind, Action<string?> OnDone);
