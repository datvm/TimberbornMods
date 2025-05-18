using ZiplineFac = Timberborn.ZiplineSystemUI.ZiplineConnectionButtonFactory;
namespace Ziporter.UI;

public class ZiporterConnectionButtonFactory(
    ZiplineFac zipline,
    Highlighter highlighter,
    ZiporterConnectionService ziporterConnectionService,
    ILoc t,
    ZiporterConnectionTool tool,
    ToolManager toolManager,
    EntityBadgeService badgeService
)
{

    public void CreateEmpty(VisualElement parent) => zipline.CreateEmpty(parent);

    public Button CreateConnection(VisualElement parent, ZiporterConnection from, ZiporterConnection to)
    {
        var btn = zipline.Create(parent);
        SetForConnection(from, to, btn);
        return btn;
    }

    public Button CreateAddConnection(VisualElement parent, ZiporterConnection from)
    {
        var button = zipline.Create(parent);
        button.AddAction(() => AddConnection(from));

        ZiplineFac.SetName(button, t.T("LV.Ziporter.ConnectHub"));
        ZiplineFac.SetIcon(button, null, ZiplineFac.PlusIconClass);
        zipline.SetRemoveConnectionButton(button);

        return button;
    }

    public void SetForConnection(ZiporterConnection from, ZiporterConnection to, Button button)
    {
        button.RegisterCallback<MouseEnterEvent>(delegate
        {
            Highlight(to);
        });
        button.RegisterCallback<MouseLeaveEvent>(delegate
        {
            Unhighlight(to);
        });
        button.RegisterCallback<DetachFromPanelEvent>(delegate
        {
            Unhighlight(to);
        });
        button.RegisterCallback<ClickEvent>(delegate
        {
            Select(to);
        });

        var badge = badgeService.GetHighestPriorityEntityBadge(to);
        ZiplineFac.SetName(button, badge.GetEntityName());
        ZiplineFac.SetIcon(button, badge.GetEntityAvatar());
        zipline.SetRemoveConnectionButton(button, delegate
        {
            RemoveConnection(from, to);
        });
    }

    void AddConnection(ZiporterConnection from)
    {
        tool.From = from;
        toolManager.SwitchTool(tool);
    }

    void Highlight(ZiporterConnection to)
    {
        highlighter.HighlightPrimary(to, zipline._ziplineSystemColorsSpec.ConnectableColor);
    }

    void Unhighlight(ZiporterConnection to)
    {
        highlighter.UnhighlightPrimary(to);
    }

    void Select(BaseComponent target)
    {
        zipline._entitySelectionService.SelectAndFocusOn(target);
    }

    void RemoveConnection(ZiporterConnection owner, ZiporterConnection otherZiplineTower)
    {
        Unhighlight(otherZiplineTower);
        ziporterConnectionService.Disconnect(otherZiplineTower);
        zipline._entitySelectionService.Unselect();
        zipline._entitySelectionService.Select(owner);
    }

}
