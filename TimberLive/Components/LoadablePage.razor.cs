namespace TimberLive.Components;

partial class LoadablePage<TModel>
    where TModel : class
{

    [Parameter, EditorRequired]
    public TModel? Model { get; set; }

    [Parameter, EditorRequired]
    public RenderFragment<TModel> ChildContent { get; set; }

    [Parameter, EditorRequired]
    public string Title { get; set; }

    [Parameter]
    public bool? Ready { get; set; }

}
