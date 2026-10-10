namespace BeaverController.UI;

[BindFragment]
public class CharacterControllerFragment(
    ILoc t,
    InputService input,
    CursorService cursor,
    CharacterControlDestinationPicker picker,
    CharacterAssignmentTool assignment
) : BaseEntityPanelFragment<CharacterControllerComponent>, IInputProcessor
{
    const string AlternateClickableActionKey = "AlternateClickableAction";
    const string PickCursorKey = "PickDestinationCursor";
    const string PickDestinationKey = "LV.BC.PickDestination";

    NineSliceButton btnRelease = null!;
    NineSliceButton btnWorkplace = null!;
    NineSliceButton btnHousing = null!;
    Toggle chkKeep = null!;
    Label lblAction = null!;
    Label lblStatus = null!;

    DestinationPick destinationPick;
    string? fragmentMessage;
    bool alternative;
    bool refreshing;

    protected override void InitializePanel()
    {
        var actionPanel = panel.AddChild().SetMarginBottom();
        lblAction = actionPanel.AddGameLabel().SetMarginBottom(5);
        AddButton(actionPanel, "LV.BC.Stop", OnStop);

        var movePanel = panel.AddChild().SetMarginBottom();
        AddButton(movePanel, "LV.BC.Move", () => StartDestinationPick(DestinationPick.Move));
        AddButton(movePanel, "LV.BC.Use", OnUse);
        chkKeep = movePanel.AddGamePanelToggle(t.T("LV.BC.Keep"), OnKeep).SetMarginBottom(5);
        btnRelease = AddButton(movePanel, "LV.BC.Release", OnRelease).SetDisplay(false);
        lblStatus = movePanel.AddGameLabel().SetMarginBottom(5);

        var assignPanel = panel.AddChild().SetMarginBottom();
        AddButton(assignPanel, "LV.BC.AssignDistrict", () => OnAssign(AssignmentKind.District));
        btnWorkplace = AddButton(assignPanel, "LV.BC.AssignWorkplace", () => OnAssign(AssignmentKind.Workplace));
        btnHousing = AddButton(assignPanel, "LV.BC.AssignHousing", () => OnAssign(AssignmentKind.Dwelling));

        var teleport = panel.AddCollapsiblePanel(t.T("LV.BC.Teleport"), expand: false).SetMargin(top: 10);
        teleport.Container.AddGameButtonPadded(
            t.T("LV.BC.TeleportHere"),
            () => StartDestinationPick(DestinationPick.Teleport),
            stretched: true);

        NineSliceButton AddButton(VisualElement parent, string key, Action onClick)
            => parent.AddGameButtonPadded(t.T(key), onClick, stretched: true).SetMarginBottom(5);
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (component is null) { return; }
        if (!component.Character.Alive)
        {
            panel.Visible = false;
            component = null;
            return;
        }

        input.RemoveInputProcessor(this);
        input.AddInputProcessor(this);
        destinationPick = DestinationPick.None;
        fragmentMessage = null;
        alternative = input.IsKeyHeld(AlternateClickableActionKey);
        RefreshAssignmentLabels();
        RefreshButtons();
        ApplyAction();
        ApplyStatus();
    }

    public override void ClearFragment()
    {
        input.RemoveInputProcessor(this);
        EndDestinationPick();
        assignment.Cancel();
        fragmentMessage = null;
        base.ClearFragment();
    }

    public override void UpdateFragment()
    {
        if (component is null) { return; }

        RefreshButtons();
        ApplyAction();
        ApplyStatus();
    }

    public bool ProcessInput()
    {
        var alt = input.IsKeyHeld(AlternateClickableActionKey);
        if (alt != alternative)
        {
            alternative = alt;
            RefreshAssignmentLabels();
        }

        if (destinationPick == DestinationPick.None) { return false; }
        if (input.Cancel)
        {
            EndDestinationPick();
            return true;
        }

        if (input.MouseOverUI || !input.MainMouseButtonDown) { return false; }

        var current = component;
        var grid = picker.PickDestination();
        if (grid is Vector3 destination && current is not null)
        {
            if (destinationPick == DestinationPick.Move)
            {
                current.CommandMove(destination);
            }
            else
            {
                current.Teleport(destination);
            }
        }

        EndDestinationPick();
        return true;
    }

    void OnKeep(bool keep)
    {
        if (refreshing || component is null) { return; }
        component.SetKeep(keep);
        btnRelease.SetDisplay(keep);
    }

    void OnStop() => component?.Stop();

    void OnUse()
    {
        if (component is null) { return; }

        EndDestinationPick();
        SetMessage("LV.BC.PickUse");
        assignment.Pick(component, AssignmentKind.Use, SetMessage);
    }

    void OnRelease() => component?.Release();

    void OnAssign(AssignmentKind kind)
    {
        if (component is null) { return; }

        EndDestinationPick();
        if (alternative && kind != AssignmentKind.District)
        {
            assignment.Unassign(component, kind);
            ClearMessage();
            return;
        }

        SetMessage(PickKey(kind));
        assignment.Pick(component, kind, SetMessage);
    }

    void StartDestinationPick(DestinationPick pick)
    {
        if (component is null) { return; }

        assignment.Cancel();
        destinationPick = pick;
        cursor.SetCursor(PickCursorKey);
        SetMessage(PickDestinationKey);
    }

    void EndDestinationPick()
    {
        if (destinationPick == DestinationPick.None) { return; }

        destinationPick = DestinationPick.None;
        cursor.ResetCursor();
        if (fragmentMessage == PickDestinationKey)
        {
            ClearMessage();
        }
    }

    void RefreshButtons()
    {
        if (component is null) { return; }

        refreshing = true;
        chkKeep.SetValueWithoutNotify(component.KeepAfterMove);
        refreshing = false;

        btnRelease.SetDisplay(component.KeepAfterMove);
        btnRelease.SetEnabled(component.IsUnderControl);
        btnHousing.SetEnabled(component.GetComponent<Dweller>());
        btnWorkplace.SetEnabled(component.GetComponent<Worker>());
    }

    void RefreshAssignmentLabels()
    {
        btnHousing.text = t.T(alternative ? "LV.BC.UnassignHousing" : "LV.BC.AssignHousing");
        btnWorkplace.text = t.T(alternative ? "LV.BC.UnassignWorkplace" : "LV.BC.AssignWorkplace");
    }

    void SetMessage(string? key)
    {
        fragmentMessage = key;
        ApplyStatus();
    }

    void ClearMessage() => SetMessage(null);

    void ApplyAction()
    {
        var name = component?.CurrentActionName;
        var action = string.IsNullOrEmpty(name) ? t.T("LV.BC.NoAction") : name;
        lblAction.text = $"{t.T("LV.BC.CurrentAction")}: {action}";
    }

    void ApplyStatus()
    {
        var key = fragmentMessage ?? component?.StatusKey ?? "LV.BC.Idle";
        if (key == "LV.BC.MovingTo")
        {
            var target = component?.UseEnterable;
            var name = target ? target!.GetName(t) : null;

            lblStatus.text = string.IsNullOrEmpty(name) ? t.T("LV.BC.Moving") : t.T(key, name);
            return;
        }

        lblStatus.text = t.T(key);
    }



    static string PickKey(AssignmentKind kind) => kind switch
    {
        AssignmentKind.Dwelling => "LV.BC.PickDwelling",
        AssignmentKind.Workplace => "LV.BC.PickWorkplace",
        AssignmentKind.District => "LV.BC.PickDistrict",
        AssignmentKind.Use => "LV.BC.PickUse",
        _ => "LV.BC.PickDistrict",
    };
}

enum DestinationPick
{
    None,
    Move,
    Teleport,
}
