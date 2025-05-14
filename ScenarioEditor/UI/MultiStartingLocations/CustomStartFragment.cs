
namespace ScenarioEditor.UI.MultiStartingLocations;

public class CustomStartFragment(
    ILoc t
) : IEntityPanelFragment
{

#nullable disable
    EntityPanelFragmentElement Root;
    Toggle chkCustom;
    IntegerField txtAdult, txtChidlren, txtFood, txtWater;
#nullable enable
    CustomStartComponent? comp;

    public void ClearFragment()
    {
        Root.Visible = false;
        comp = null;
    }

    public VisualElement InitializeFragment()
    {
        Root = new()
        {
            Background = EntityPanelFragmentBackground.Green,
            Visible = false,
        };

        chkCustom = Root.AddToggle("LV.ScE.CustomStart".T(t), onValueChanged: OnEnabledChanged).SetMarginBottom();
        txtAdult = CreateIntField(Root, "NewGameConfigurationPanel.StartingAdults", v => comp!.Parameters.Adult = v);
        txtChidlren = CreateIntField(Root, "NewGameConfigurationPanel.StartingChildren", v => comp!.Parameters.Children = v);
        txtFood = CreateIntField(Root, "NewGameConfigurationPanel.StartingFood", v => comp!.Parameters.Food = v);
        txtWater = CreateIntField(Root, "NewGameConfigurationPanel.StartingWater", v => comp!.Parameters.Water = v);

        return Root;
    }

    IntegerField CreateIntField(VisualElement parent, string loc, Action<int> callback)
    {
        var row = parent.AddRow().AlignItems().SetMarginBottom();

        row.AddGameLabel(loc.T(t)).SetMarginRight();
        return row
            .AddIntField(changeCallback: v =>
            {
                if (!comp) { return; }
                callback(v);
            })
            .SetFlexGrow();
    }

    public void ShowFragment(BaseComponent entity)
    {
        comp = entity.GetComponentFast<CustomStartComponent>();
        if (comp is null) { return; }

        UpdateContent();
        Root.Visible = true;
    }

    void OnEnabledChanged(bool enabled)
    {
        comp!.Parameters.Enabled = enabled;
        SetUIEnabled();
    }

    void UpdateContent()
    {
        var p = comp!.Parameters;

        chkCustom.SetValueWithoutNotify(p.Enabled);
        txtAdult.SetValueWithoutNotify(p.Adult);
        txtChidlren.SetValueWithoutNotify(p.Children);
        txtFood.SetValueWithoutNotify(p.Food);
        txtWater.SetValueWithoutNotify(p.Water);

        SetUIEnabled();
    }

    void SetUIEnabled()
    {
        txtAdult.SetEnabled(chkCustom.value);
        txtChidlren.SetEnabled(chkCustom.value);
        txtFood.SetEnabled(chkCustom.value);
        txtWater.SetEnabled(chkCustom.value);
    }

    public void UpdateFragment() { }
}
