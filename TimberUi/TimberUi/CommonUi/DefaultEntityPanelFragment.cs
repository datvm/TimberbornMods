namespace TimberUi.CommonUi;

public abstract class BaseEntityPanelFragment<T> : IEntityPanelFragment
    where T : BaseComponent
{

    protected EntityPanelFragmentElement panel = null!;
    protected T? component;

    public VisualElement InitializeFragment()
    {
        panel = new EntityPanelFragmentElement();
        InitializePanel();
        return panel;
    }
    protected abstract void InitializePanel();

    public virtual void ClearFragment()
    {
        panel.Visible = false;
        component = null;
    }

    public virtual void ShowFragment(BaseComponent entity)
    {
        component = entity.GetComponent<T>();
        if (!component)
        {
            component = null;
            return;
        }
        panel.Visible = true;
    }

    public virtual void UpdateFragment() { }
}
