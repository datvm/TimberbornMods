namespace TimberUi.CommonProviders;

public class EntityPanelFragmentProvider<T>(T fragment) : EntityPanelFragmentProvider([fragment]) where T : IEntityPanelFragment
{

}

public class EntityPanelFragmentProvider(IEnumerable<EntityPanelRegistration> fragments) : IProvider<EntityPanelModule>
{
    public const EntityPanelFragmentPosition DefaultPosition = EntityPanelFragmentPosition.Top;

    public EntityPanelFragmentProvider(IEnumerable<IEntityPanelFragment> fragments)
        : this(fragments, DefaultPosition) { }

    public EntityPanelFragmentProvider(IEnumerable<IEntityPanelFragment> fragments, EntityPanelFragmentPosition position)
        : this(fragments.Select(q => new EntityPanelRegistration(q, position))) { }

    public EntityPanelModule Get()
    {
        EntityPanelModule.Builder builder = new();

        foreach (var f in fragments)
        {
            switch (f.Position)
            {
                case EntityPanelFragmentPosition.Top:
                    builder.AddTopFragment(f.Fragment);
                    break;
                case EntityPanelFragmentPosition.Bottom:
                    builder.AddBottomFragment(f.Fragment);
                    break;
                case EntityPanelFragmentPosition.Diagnostic:
                    builder.AddDiagnosticFragment(f.Fragment);
                    break;
                case EntityPanelFragmentPosition.Footer:
                    builder.AddFooterFragment(f.Fragment);
                    break;
                case EntityPanelFragmentPosition.LeftHeader:
                    builder.AddLeftHeaderFragment(f.Fragment);
                    break;
                case EntityPanelFragmentPosition.Middle:
                    builder.AddMiddleFragment(f.Fragment);
                    break;
                case EntityPanelFragmentPosition.RightHeader:
                    builder.AddRightHeaderFragment(f.Fragment);
                    break;
                case EntityPanelFragmentPosition.Side:
                    builder.AddSideFragment(f.Fragment);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(f.Position), f.Position, "Unknown value");
            }
        }

        return builder.Build();
    }
}

public readonly record struct EntityPanelRegistration(IEntityPanelFragment Fragment, EntityPanelFragmentPosition Position);

public enum EntityPanelFragmentPosition
{
    Top,
    Bottom,
    Diagnostic,
    Footer,
    LeftHeader,
    Middle,
    RightHeader,
    Side,
}