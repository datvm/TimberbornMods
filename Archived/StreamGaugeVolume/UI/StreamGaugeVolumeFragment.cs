namespace StreamGaugeVolume.UI;

public class StreamGaugeVolumeFragment(UIBuilder builder, ILoc loc) : IEntityPanelFragment
{
#nullable disable
    VisualElement root;
    NineSliceLabel lblVolume, lblCells;
    Button btnMeasure;
#nullable enable

    StreamGaugeVolumeComponent? curr;

    public void ClearFragment()
    {
        curr = null;
        SetVisibility(root, false);
    }

    public VisualElement InitializeFragment()
    {
        root = builder.BuildAndInitialize<StreamGaugeVolumeFragmentBuilder>();

        lblVolume = root.Q<NineSliceLabel>("Volume");
        lblCells = root.Q<NineSliceLabel>("CellsCount");

        btnMeasure = root.Q<Button>("MeasureVolume");
        btnMeasure.text = loc.T("LV.SGV.Measure");
        btnMeasure.RegisterCallback<ClickEvent>(_ => curr?.Measure());

        SetVisibility(root, false);
        return root;
    }

    public void ShowFragment(BaseComponent entity)
    {
        curr = entity.GetComponentFast<StreamGaugeVolumeComponent>();

        SetVisibility(root, curr is not null);
    }

    public void UpdateFragment()
    {
        if (curr is null) { return; }

        var volume = curr?.Volume;
        
        lblVolume.text = loc.T("LV.SGV.Volume", FormatVolume(volume?.Volume) ?? "N/A");
        lblCells.text = loc.T("LV.SGV.CellsCount", volume?.Cells.ToString() ?? "N/A");
    }

    static string? FormatVolume(float? volume) => volume is null ? null : $"{volume:F2} cm³";

    private static void SetVisibility(VisualElement element, bool visible)
    {
        element.visible = visible;
        element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
