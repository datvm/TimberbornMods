namespace TImprove.Services;

public class CoordsService(
    MSettings s,
    CoordsPanel panel,
    ILoc loc,
    InputService input,
    CursorCoordinatesPicker picker)
    : IInputProcessor, ILoadableSingleton, IUnloadableSingleton
{
    bool show, heightOnly;

    public void Load()
    {
        input.AddInputProcessor(this);

        s.OnSettingsChanged += UpdateValues;
        UpdateValues();
    }

    void UpdateValues()
    {
        show = s.ShowCoords;
        heightOnly = s.OnlyShowHeight;

        panel.SetVisibility(show);
    }

    public bool ProcessInput()
    {
        if (!show) { return false; }

        var coords = picker.Pick();
        string text;
        if (coords.HasValue)
        {
            var c = coords.Value;

            if (heightOnly)
            {
                text = loc.T("LV.TI.HeightLabel",
                    c.TileCoordinates.z.ToString());
            }
            else
            {
                text = loc.T("LV.TI.CoordsLabel",
                    c.TileCoordinates.x.ToString("D2"),
                    c.TileCoordinates.y.ToString("D2"),
                    c.TileCoordinates.z.ToString());
            }
        }
        else
        {
            text = "N/A";
        }
        panel.SetText(text);

        return false;
    }

    public void Unload()
    {
        input.RemoveInputProcessor(this);
    }
}