namespace Pin.UI;

public class PinBatchControlRow : NineSliceButton
{

    readonly Image icon;
    readonly Label lblText;
    PinComponent? pin;

    public PinBatchControlRow(PinService pinService, EntitySelectionService entitySelectionService)
    {
        this.AddClasses(["batch-control-box__row", "bg-box--green"]);

        icon = this.AddImage().SetMarginRight().SetSize(32);
        icon.sprite = pinService.PinSprite;

        lblText = this.AddGameLabel().SetFlexShrink();
        lblText.style.overflow = Overflow.Hidden;
        lblText.style.textOverflow = TextOverflow.Ellipsis;

        RegisterCallback<ClickEvent>(_ =>
        {
            if (pin)
            {
                entitySelectionService.SelectAndFocusOn(pin);
            }
        });
    }

    public void SetPin(PinComponent pin)
    {
        lblText.style.color = icon.tintColor = pin.Color;
        lblText.text = pin.Label;
        
        this.pin = pin;
    }

}
