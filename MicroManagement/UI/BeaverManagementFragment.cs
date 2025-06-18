namespace MicroManagement.UI;

public class BeaverManagementFragment(ILoc t, BeaverAssignmentTool tool, InputService input) : IEntityPanelFragment, IInputProcessor
{
    public const string AlternateClickableActionKey = "AlternateClickableAction";

    Citizen? citizen;

#nullable disable
    EntityPanelFragmentElement panel;

    Button btnAssignHousing;
    Button btnAssignWorkplace;
    Label lblStatus;
#nullable enable

    bool isAlternative;

    public VisualElement InitializeFragment()
    {
        panel = new()
        {
            Background = EntityPanelFragmentBackground.Blue,
            Visible = false,
        };

        btnAssignHousing = AddGameButton("LV.MM.AssignHouse", CitizenAssignmentType.Dwelling);
        btnAssignWorkplace = AddGameButton("LV.MM.AssignWorkplace", CitizenAssignmentType.Workplace);
        AddGameButton("LV.MM.AssignDistrict", CitizenAssignmentType.District);

        lblStatus = panel.AddGameLabel("", "Status")
            .SetMargin(top: 10);

        return panel;

        Button AddGameButton(string locKey, CitizenAssignmentType type) 
        {
            return panel.AddGameButton(t.T(locKey), onClick: () => Assign(type))
                .SetFlexGrow(1)
                .SetPadding(paddingY: 5)
                .SetMarginBottom(5);
        }
    }

    public void ClearFragment()
    {
        input.RemoveInputProcessor(this);
        lblStatus.visible = false;
        panel.Visible = false;
    }

    public void ShowFragment(BaseComponent entity)
    {
        citizen = entity.GetComponentFast<Citizen>();
        if (citizen is null) { return; }

        input.AddInputProcessor(this);

        btnAssignHousing.enabledSelf = citizen.GetComponentFast<Dweller>();
        btnAssignWorkplace.enabledSelf = citizen.GetComponentFast<Worker>();

        panel.Visible = true;
    }

    public void UpdateFragment() { }

    void Assign(in CitizenAssignmentType type)
    {
        if (citizen is null) { return; }

        if (isAlternative && type != CitizenAssignmentType.District)
        {
            tool.Unassign(citizen, type);
        }
        else
        {
            lblStatus.text = t.T("LV.MM.Pick" + type);
            lblStatus.visible = true;

            tool.PickAssignment(new(citizen, type, (err) =>
            {
                if (err is null)
                {
                    lblStatus.visible = false;
                }
                else
                {
                    lblStatus.text = t.T(err);
                }
            }));
        }
    }

    public bool ProcessInput()
    {
        var alternativeKey = input.IsKeyHeld(AlternateClickableActionKey);
        if (alternativeKey != isAlternative)
        {
            isAlternative = alternativeKey;
            SetButtonTexts();
        }

        return false;
    }

    void SetButtonTexts()
    {
        if (isAlternative)
        {
            btnAssignHousing.text = t.T("LV.MM.UnassignHouse");
            btnAssignWorkplace.text = t.T("LV.MM.UnassignWorkplace");
        }
        else
        {
            btnAssignHousing.text = t.T("LV.MM.AssignHouse");
            btnAssignWorkplace.text = t.T("LV.MM.AssignWorkplace");
        }
    }

}
