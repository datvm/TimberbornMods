namespace MicroManagement.UI;

public class SendAllToDistrictCenterFragment(
    ILoc t,
    BeaverAssignmentService service,
    DialogBoxShower diagConfirm
) : IEntityPanelFragment
{

#nullable disable
    EntityPanelFragmentElement panel;
#nullable enable
    DistrictCenter? comp;

    public void ClearFragment()
    {
        panel.Visible = false;
        comp = null;
    }

    public VisualElement InitializeFragment()
    {
        panel = new()
        {
            Visible = false,
        };

        panel.AddGameLabel(t.T("LV.MM.SendAll")).SetMarginBottom(5);
        foreach (var target in BeaverAssignmentService.AllTargetTypes)
        {
            panel.AddGameButton(t.T("LV.MM.Send" + target), onClick: () => AssignAll(target), stretched: true)
                .SetPadding(5).SetMarginBottom(5);
        }

        return panel;
    }

    public void ShowFragment(BaseComponent entity)
    {
        comp = entity.GetComponentFast<DistrictCenter>();
        if (!comp) { return; }

        panel.Visible = true;
    }

    public void UpdateFragment()
    {
    }

    void AssignAll(BeaverAssignmentTarget target)
    {
        var district = comp;
        if (!district) { return; }

        var beavers = service.GetBeavers(target).ToArray();
        var alreadyAssigned = beavers
            .Where(b => b.HasAssignedDistrict && b.AssignedDistrict == district)
            .ToHashSet();

        if (beavers.Length == 0 || beavers.Length == alreadyAssigned.Count)
        {
            diagConfirm.Create()
                .SetLocalizedMessage("LV.MM.SendAllNoBeavers")
                .Show();
            return;
        }
        else
        {
            var districtName = district.DistrictName;

            diagConfirm.Create()
                .SetMessage(t.T("LV.MM.SendAllConfirm", beavers.Length - alreadyAssigned.Count, districtName, alreadyAssigned.Count))
                .SetConfirmButton(() => service.Assign(beavers, district))
                .SetDefaultCancelButton()
                .Show();
        }
    }
}
