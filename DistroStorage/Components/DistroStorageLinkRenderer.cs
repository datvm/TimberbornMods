
namespace DistroStorage.Components;

[AddTemplateModule2(typeof(DistroStorageComponent))]
public class DistroStorageLinkRenderer(DistroLinkRenderer renderer) : BaseComponent, IAwakableComponent, ISelectionListener
{

#nullable disable
    DistroStorageComponent distro;
#nullable enable

    public void Awake()
    {
        distro = GetComponent<DistroStorageComponent>();
    }

    public void OnSelect()
    {
        var center = distro.AboveCenter;

        RenderComponent(distro.Sender, center, renderer.AddOutputLine);
        RenderComponent(distro.Receiver, center, renderer.AddInputLine);
    }

    void RenderComponent(IDistroComponent? comp, Vector3 center, Action<Vector3, Vector3> render)
    {
        if (comp is null || !comp.ActiveAndEnabled) { return; }

        foreach (var conn in comp.Connections)
        {
            if (!conn.ActiveAndEnabled) { continue; }

            var other = ((BaseComponent)conn).GetComponent<DistroStorageComponent>();
            render(center, other.AboveCenter);
        }
    }

    public void OnUnselect() => renderer.Clear();

}
