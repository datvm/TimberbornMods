namespace TimberUi;

public class BindSingletonAttribute : BindAttribute
{

    public BindSingletonAttribute()
    {
        Scope = Bindito.Core.Internal.Scope.Singleton;
    }

}

public class BindTransientAttribute : BindAttribute
{
    public BindTransientAttribute()
    {
        Scope = Bindito.Core.Internal.Scope.Transient;
    }
}

public class MultiBindAttribute : BindAttribute
{
    public MultiBindAttribute(Type @as)
    {
        MultiBind = true;
        As = @as;
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class BindFragmentAttribute : Attribute
{

}
