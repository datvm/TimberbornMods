namespace TimberUi.CommonUi;

public class ClosableAlertElement
{

    public readonly VisualElement Root;
    public readonly Button Button;
    public readonly Button CloseButton;

    public bool Visible
    {
        get => Root.IsDisplayed();
        set => Root.SetDisplay(value);
    }

    public ClosableAlertElement(AlertPanelRowFactory factory, string iconName)
    {
        Root = factory.CreateClosable(iconName);        
        Button = Root.Q<Button>("Button");
        CloseButton = Root.Q<Button>("Close");

        Visible = true;
    }

    public ClosableAlertElement AddCloseCallback(Action onClose)
    {
        CloseButton.AddAction(onClose);
        return this;
    }
    
    public ClosableAlertElement SetButtonCallback(Action onClick)
    {
        Button.AddAction(onClick);
        return this;
    }

    public ClosableAlertElement SetButtonAsCloseCallback()
        => SetButtonCallback(() => Visible = false);

    public ClosableAlertElement SetText(string text)
    {
        Button.text = text;
        return this;
    }



}
