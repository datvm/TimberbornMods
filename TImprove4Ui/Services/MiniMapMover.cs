namespace TImprove4Ui.Services;

[BindSingleton]
public class MiniMapMover(
    MSettings s,
    UILayout uiLayout
) : ILoadableSingleton
{
    
    public void Load()
    {
        s.MoveBottomRightUp.ValueChanged += OnValueChanged;
        OnValueChanged(null!, s.MoveBottomRightUp.Value);
    }

    void OnValueChanged(object _, bool e)
    {
        uiLayout._bottomRight.style.bottom = e ? 80 : 0;
    }
}
