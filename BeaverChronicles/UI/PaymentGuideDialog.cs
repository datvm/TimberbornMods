namespace BeaverChronicles.UI;

[BindSingleton]
public class PaymentGuideDialog(
    ISingletonLoader loader,
    ILoc t,
    IAssetLoader assets,
    VisualElementInitializer veInit, PanelStack panelStack,
    EventBus eb
) : DialogBoxElement, ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new("ChronicleEventsPaymentGuideDialog");
    static readonly PropertyKey<bool> DontShowAgainKey = new("DontShowAgain");

    public bool DontShowAgain { get; set; }

    public void Load()
    {
        if (loader.TryGetSingleton(SaveKey, out var s) && s.Has(DontShowAgainKey))
        {
            DontShowAgain = s.Get(DontShowAgainKey);
        }

        if (!DontShowAgain)
        {
            Initialize();
            eb.Register(this);
        }
    }

    [OnEvent]
    public void OnNewPayment(OnNewChroniclePaymentEvent _)
    {
        ShowIfNeeded();
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        if (DontShowAgain)
        {
            var s = singletonSaver.GetSingleton(SaveKey);
            s.Set(DontShowAgainKey, true);
        }
    }

    void Initialize()
    {
        SetTitle(t.T("LV.BCEv.PayGuide.Title"));
        AddCloseButton();
        var parent = Content;
        parent.AddLabel(t.T("LV.BCEv.PayGuide.Content")).SetMarginBottom(10);
        parent.AddImage<Texture2D>("Resources/Chronicles/UI/payment_guide", assets).SetMaxSizePercent(100, null)
            .SetMarginBottom();

        parent.AddToggle(t.T("LV.TimberUi.UpdateDismiss"), onValueChanged: OnDontShowChanged).style.alignSelf = Align.Center;
    }

    void OnDontShowChanged(bool v)
    {
        DontShowAgain = v;
    }

    public void ShowIfNeeded()
    {
        if (DontShowAgain) { return; }
        Show(veInit, panelStack, OnClosed, OnClosed);
    }

    void OnClosed()
    {
        if (DontShowAgain)
        {
            eb.Unregister(this);
        }
    }

}
