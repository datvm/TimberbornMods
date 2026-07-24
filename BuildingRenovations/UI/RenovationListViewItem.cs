namespace BuildingRenovations.UI;

public class RenovationListViewItem : NineSliceVisualElement
{
    readonly Label lbl;
    public RenovationListItemModel Model { get; }

    public RenovationListViewItem(RenovationListItemModel model, Action<RenovationListViewItem> callback, IContainer container)
    {
        Model = model;

        this.SetAsRow().AlignItems().SetMarginBottom();
        var spec = model.Renovation.Spec;
        
        var avatar = this.AddChild(container.GetInstance<FramedAvatar>).SetMarginRight(5);
        avatar.SetIcon(spec.Icon?.Asset, 48);

        lbl = this.AddGameLabel(model.Renovation.Spec.Title.Value).SetWrap()
            .SetPadding(left: 10, top: 10, bottom: 10);

        if (!model.IsAvailable)
        {
            lbl.style.color = Color.gray;
        }

        RegisterCallback<ClickEvent>(_ => callback(this));
    }

    public void Select() => lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
    public void Unselect() => lbl.style.unityFontStyleAndWeight = FontStyle.Normal;

    public bool Filter(RenovationDialogFilter filter)
    {
        var keyword = filter.Keyword;
        var match = string.IsNullOrEmpty(keyword)
            || Model.Renovation.Spec.Title.Value.Contains(keyword, StringComparison.OrdinalIgnoreCase);

        // Soft-unavailable (Applicable but has a reason) always stays listed.
        // Hard-unavailable (!Applicable) only when the toggle is on.
        match = match && (Model.Applicable || filter.ShowUnavailables);

        this.SetDisplay(match);
        return match;
    }
}

/// <param name="Applicable"><see cref="RenovationBase.CanRenovate"/> — hard filter.</param>
/// <param name="NotAvailableReason">Soft unavailability reason, or not-applicable text when hard-filtered.</param>
public readonly record struct RenovationListItemModel(
    RenovationBase Renovation,
    bool Applicable,
    string? NotAvailableReason
)
{
    public bool IsAvailable => Applicable && NotAvailableReason is null;
}
