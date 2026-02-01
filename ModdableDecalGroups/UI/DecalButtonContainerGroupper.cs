
namespace ModdableDecalGroups.UI;

[BindSingleton]
public class DecalButtonContainerGroupper
{
    public const string ContainerName = "DecalGroups";

    static WeakReference<DecalButtonContainerGroupper>? weakRef;
    readonly DecalGroupService groupService;

    public static DecalButtonContainerGroupper? Instance => weakRef?.TryGetTarget(out var target) == true ? target : null;

    public DecalButtonContainerGroupper(DecalGroupService groupService)
    {
        this.groupService = groupService;
        weakRef = new(this);
    }

    VisualElement RemoveOriginalScroll(VisualElement root)
    {
        while (true)
        {
            if (root.name == ContainerName) { return root; }
            if (root.name == "ButtonsScrollView") { break; }

            root = root.parent;

            if (root is null)
            {
                throw new InvalidOperationException("Could not find ButtonsScrollView in hierarchy.");
            }
        }

        var parent = root.parent;
        root.RemoveFromHierarchy();

        var subPanel = parent;
        while (true)
        {
            if (subPanel.classList.Contains("entity-sub-panel"))
            {
                break;
            }

            if (subPanel.parent is null)
            {
                subPanel.PrintVisualTree(true);
                throw new InvalidOperationException("Could not find entity-sub-panel in hierarchy.");
            }

            subPanel = subPanel.parent;
        }
        subPanel.style.maxHeight = new StyleLength(StyleKeyword.None);

        // Search for CollapsiblePanel made by T4UX
        var collapsible = subPanel.Q<CollapsiblePanel>();  
        if (collapsible is not null)
        {
            parent = collapsible.Container;
        }

        return parent.AddChild(name: ContainerName);
    }

    public void RemoveButtons(DecalButtonContainer container)
    {
        container._root.Clear();
        container._decalButtons.Clear();
    }

    public void Show(DecalButtonContainer container, DecalSupplier decalSupplier)
    {
        RemoveButtons(container);

        var root = container._root = RemoveOriginalScroll(container._root);

        foreach (var grp in groupService.GetGroups(decalSupplier.Category).Groups)
        {

            var panel = root.AddChild<CollapsiblePanel>();
            panel.SetTitle(grp.Spec.Title.Value);
            panel.SetExpand(grp.Spec.IsDefault);

            var grpContainer = panel.Container.AddRow();
            grpContainer.SetWrap().SetMargin(top: 20);

            foreach (var decal in grp.Decals)
            {
                var btn = container._decalButtonFactory.CreateButton(decal);
                btn.Show(decalSupplier);
                container._decalButtons.Add(btn);
                grpContainer.Add(btn.Root);
            }
        }
    }

}
