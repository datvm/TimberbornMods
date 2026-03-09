namespace TimberLive.Components;

partial class AccordionItem
{

    [Parameter]
    public bool InitiallyExpanded { get; set; } = true;

    [Parameter, EditorRequired]
    public string Title { get; set; }

    [Parameter]
    public RenderFragment? Header { get; set; }

    [Parameter, EditorRequired]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool IsOpen { get; set; } = true;

    [Parameter]
    public EventCallback<bool> IsOpenChanged { get; set; }

    bool initialized;
    bool isOpen;

    protected override void OnParametersSet()
    {
        if (!initialized)
        {
            isOpen = InitiallyExpanded;
            initialized = true;
        }
        else
        {
            isOpen = IsOpen;
        }
    }

    public async Task ToggleAsync()
    {
        isOpen = !isOpen;
        await IsOpenChanged.InvokeAsync(isOpen);
    }

    public async Task OpenAsync()
    {
        if (isOpen) { return; }
        await ToggleAsync();
    }

    public async Task CloseAsync()
    {
        if (!isOpen) { return; }
        await ToggleAsync();
    }

}
