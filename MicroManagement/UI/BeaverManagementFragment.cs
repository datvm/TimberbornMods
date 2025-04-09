namespace MicroManagement.UI;

public class BeaverManagementFragment(ILoc t, BeaverAssignmentTool tool, InputService input) : IEntityPanelFragment, IInputProcessor
{
    public const string AlternateClickableActionKey = "AlternateClickableAction";

    EntityPanelFragmentElement panel = null!;
    Citizen? citizen;

    Button btnAssignHousing = null!;
    Button btnAssignWorkplace = null!;
    Label lblStatus = null!;

    bool isAlternative;

    public VisualElement InitializeFragment()
    {
        panel = new()
        {
            Background = EntityPanelFragmentBackground.Blue,
        };

        btnAssignHousing = panel.AddGameButton(t.T("LV.MM.AssignHouse"), () => Assign(CitizenAssignmentType.Dwelling), stretched: true);
        btnAssignWorkplace = panel.AddGameButton(t.T("LV.MM.AssignWorkplace"), () => Assign(CitizenAssignmentType.Workplace), stretched: true);
        panel.AddGameButton(t.T("LV.MM.AssignDistrict"), () => Assign(CitizenAssignmentType.District), stretched: true);

        lblStatus = panel.AddGameLabel("", "Status")
            .SetMargin(top: 10);


        return panel;
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
