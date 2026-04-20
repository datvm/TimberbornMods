namespace DistroStorage.UI;

[BindFragment]
public class DistroStorageFragment(
    ILoc t,
    EntitySelectionService selectionService,
    IGoodService goods
) : BaseEntityPanelFragment<DistroStorageComponent>
{

#nullable disable
    DistroPanel<IDistroSender> pnlSender;
    DistroPanel<IDistroReceiver> pnlReceiver;
#nullable enable

    protected override void InitializePanel()
    {
        pnlSender = Create<IDistroSender>();
        pnlReceiver = Create<IDistroReceiver>();

        panel.Add(pnlSender.SetMarginBottom());
        panel.Add(pnlReceiver);

        DistroPanel<T> Create<T>() where T : IDistroComponent
        {
            DistroPanel<T> r = new(t, goods);
            r.OnSelectRequested += OnSelectRequested;
            return r;
        }
    }

    void OnSelectRequested(BaseComponent obj) => selectionService.SelectAndFocusOn(obj);

    public override void UpdateFragment()
    {
        if (!component) { return; }

        var sender = component!.Sender;
        pnlSender.SetComponent(sender);
        pnlSender.Update();

        var receiver = component!.Receiver;
        pnlReceiver.SetComponent(receiver);
        pnlReceiver.Update();

        panel.Visible = pnlSender.Visible || pnlReceiver.Visible;
    }

    public override void ClearFragment()
    {
        pnlSender.ClearInfo();
        pnlReceiver.ClearInfo();
        base.ClearFragment();
    }

}
