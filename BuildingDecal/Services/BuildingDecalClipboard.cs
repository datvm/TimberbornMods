namespace BuildingDecal.Services;

public class BuildingDecalClipboard(
    QuickNotificationService notf,
    ILoc t
)
{
    public const string CopyDecalKey = "CopyBuildingDecal";
    public const string PasteDecalKey = "PasteBuildingDecal";

    BuildingDecalItem? copiedItem;

    public event Action? OnClipboardChanged;

    public bool CanPaste => copiedItem is not null && copiedItem.Attached;

    public void Copy(BuildingDecalComponent component)
    {
        var count = component.DecalItems.Count;
        switch (count)
        {
            case 0:
                notf.SendNotification(t.T("LV.BDl.NoDecal"), true);
                break;
            case 1:
                Copy(component.DecalItems.First(), true);
                break;
            default:
                Copy(component.DecalItems.First(), false);
                notf.SendNotification(t.T("LV.BDl.CopyOneNotf"));
                break;
        }
    }

    public void Copy(BuildingDecalItem buildingDecalItem)
    {
        Copy(buildingDecalItem, true);
    }

    void Copy(BuildingDecalItem buildingDecalItem, bool sendNotification)
    {
        copiedItem = buildingDecalItem;
        OnClipboardChanged?.Invoke();

        if (sendNotification)
        {
            notf.SendNotification(t.T("LV.BDl.CopyNotf"));
        }
    }


    public BuildingDecalItem? Paste(BuildingDecalComponent component)
    {
        if (!CanPaste)
        {
            if (copiedItem is not null)
            {
                copiedItem = null;
                OnClipboardChanged?.Invoke();
            }
            
            return null;
        }

        return component.AddDecal(copiedItem!);
    }

    public bool ProcessInput()
    {

        return false;
    }
}
