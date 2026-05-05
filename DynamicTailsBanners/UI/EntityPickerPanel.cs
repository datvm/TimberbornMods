namespace DynamicTailsBanners.UI;

[BindTransient]
public class EntityPickerPanel : VisualElement
{

    readonly Image buildingIcon;
    readonly Label buildingName;
    readonly Button btnGo, btnRemove;
    readonly ILoc t;
    readonly EntitySelectionService entitySelectionService;
    readonly NamedIconProvider namedIconProvider;
    readonly EntityBadgeService entityBadgeService;
    readonly SelectEntityTool selectEntityTool;

    public EntityComponent? SelectedEntity { get; private set; }
    public event EventHandler<(BaseComponent? Source, EntityComponent? Entity)>? OnEntityChanged;

    public BaseComponent? SourceEntity { get; set; }

    public EntityPickerPanel(
        ILoc t,
        EntitySelectionService entitySelectionService,
        NamedIconProvider namedIconProvider,
        EntityBadgeService entityBadgeService,
        SelectEntityTool selectEntityTool)
    {
        this.t = t;
        this.entitySelectionService = entitySelectionService;
        this.namedIconProvider = namedIconProvider;
        this.entityBadgeService = entityBadgeService;
        this.selectEntityTool = selectEntityTool;

        this.SetAsRow().AlignItems().SetMarginBottom(10);
        buildingIcon = this.AddImage().SetSize(20).SetMarginRight(5);
        buildingName = this.AddLabel().SetFlexGrow().SetFlexShrink();

        btnGo = this.AddGameButtonPadded(t.T("LV.DTB.GoTo"), GoToSelectedBuilding).SetMarginRight(5).SetDisplay(false);
        btnRemove = this.AddGameButtonPadded(t.T("LV.DTB.Remove"), RemoveEntity).SetMarginRight(5).SetDisplay(false);
        this.AddGameButtonPadded(t.T("LV.DTB.Pick"), PickEntity);
    }

    void GoToSelectedBuilding()
    {
        if (!SelectedEntity) { return; }
        entitySelectionService.SelectAndFocusOn(SelectedEntity);
    }

    void RemoveEntity() => SetEntity(null);

    public void SetEntity(EntityComponent? entity)
        => SetEntity(SourceEntity, entity);

    void SetEntity(BaseComponent? source, EntityComponent? entity)
    {
        if (InternalSetEntity(entity))
        {
            OnEntityChanged?.Invoke(this, (source, entity));
        }
    }

    public void SetEntityWithoutNotifying(EntityComponent? entity) => InternalSetEntity(entity);

    public void ClearWithoutNotifying()
    {
        SetEntityWithoutNotifying(null);
        SourceEntity = null;
    }

    bool InternalSetEntity(EntityComponent? entity)
    {
        if (entity == SelectedEntity) { return false; }

        SelectedEntity = entity;
        UpdateEntityUI();

        return true;
    }

    void UpdateEntityUI()
    {
        if (!SelectedEntity)
        {
            buildingIcon.sprite = namedIconProvider.QuestionMark;
            buildingName.text = t.TNone();

            btnGo.SetDisplay(false);
            btnRemove.SetDisplay(false);

            return;
        }

        buildingIcon.sprite = entityBadgeService.GetEntityAvatar(SelectedEntity);
        buildingName.text = SelectedEntity!.GetName(t);
        btnGo.SetDisplay(true);
        btnRemove.SetDisplay(true);
    }

    async void PickEntity()
    {
        var source = SourceEntity;

        var entity = await selectEntityTool.SelectAsync(new(source));
        if (!entity) { return; }

        SetEntity(source, entity);
    }

}
