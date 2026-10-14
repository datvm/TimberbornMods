namespace ConveyorBelt.UI;

[BindSingleton]
class BeltDebugFragment(DebugFragmentFactory fragments, IBlockService blocks) : IEntityPanelFragment
{
    VisualElement root = null!;
    BeltInventory? belt;

    public VisualElement InitializeFragment()
    {
        root = fragments.Create(
            new DebugFragmentButton(Fill, "Fill"),
            new DebugFragmentButton(ClearAll, "Clear all"));
        return root;
    }

    public void ShowFragment(BaseComponent entity)
    {
        belt = entity.GetComponent<BeltInventory>();
        UpdateFragment();
    }

    public void ClearFragment()
    {
        belt = null;
        UpdateFragment();
    }

    public void UpdateFragment() => root.ToggleDisplayStyle(belt);

    void Fill()
    {
        if (belt is not { } current)
        {
            return;
        }

        current.FillLogs();
    }

    void ClearAll()
    {
        if (belt is not { } start)
        {
            return;
        }

        List<BeltInventory> pending = [start];
        HashSet<BeltInventory> seen = [start];
        List<Vector3Int> links = [];
        for (var i = 0; i < pending.Count; i++)
        {
            var current = pending[i];
            current.ClearContents();
            current.CollectLinks(links);
            foreach (var cell in links)
            {
                foreach (var obj in blocks.GetObjectsAt(cell))
                {
                    if (!obj)
                    {
                        continue;
                    }

                    if (obj.GetComponent<BeltInventory>() is not { } next || !seen.Add(next))
                    {
                        continue;
                    }

                    pending.Add(next);
                }
            }
        }
    }
}
