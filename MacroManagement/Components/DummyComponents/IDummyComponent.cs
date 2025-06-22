namespace MacroManagement.Components.DummyComponents;

public interface IDummyComponent<TSelf, TComponent>
    where TSelf : TComponent, IDummyComponent<TSelf, TComponent>
    where TComponent : BaseComponent
{

    MMComponent MMComponent { get; set; }

    void Init(TComponent original);

}

public interface IWarningDummyComponent<TSelf, TComponent> : IDummyComponent<TSelf, TComponent>
    where TSelf : TComponent, IWarningDummyComponent<TSelf, TComponent>
    where TComponent : BaseComponent
{
    ILoc T { get; set; }
    DialogBoxShower DiaglogBoxShower { get; set; }

    [Inject]
    void Inject(ILoc t, DialogBoxShower diag);
}

public static class DummyComponentExtensions
{

    public static void InjectWarningDummy<TSelf, TComponent>(this IWarningDummyComponent<TSelf, TComponent> dummyComponent, ILoc t, DialogBoxShower diag)
        where TSelf : TComponent, IWarningDummyComponent<TSelf, TComponent>
        where TComponent : BaseComponent
    {
        dummyComponent.T = t;
        dummyComponent.DiaglogBoxShower = diag;
    }

    public static void Confirm<TSelf, TComponent>(this IWarningDummyComponent<TSelf, TComponent> dummyComponent, string locKey, Action action)
        where TSelf : TComponent, IWarningDummyComponent<TSelf, TComponent>
        where TComponent : BaseComponent
    {
        var count = dummyComponent.MMComponent.SelectedBuildings.Length;
        if (count == 0) { return; }

        dummyComponent.DiaglogBoxShower.Create()
            .SetMessage(dummyComponent.T.T(locKey, count))
            .SetConfirmButton(action)
            .SetDefaultCancelButton()
            .Show();
    }

    public static void Proxy<TSelf, TComponent>(this IDummyComponent<TSelf, TComponent> dummyComponent, Action<TComponent> action, bool throwIfFail = false)
        where TSelf : TComponent, IDummyComponent<TSelf, TComponent>
        where TComponent : BaseComponent
    {
        foreach (var building in dummyComponent.MMComponent.SelectedBuildings)
        {
            try
            {
                var comp = building.Prefab.GetComponentFast<TComponent>();
                if (comp)
                {
                    action(comp);
                }
            }
            catch (Exception ex)
            {
                if (throwIfFail)
                {
                    throw;
                }
                else
                {
                    Debug.LogError(ex);
                }
            }
        }
    }

    public static void Proxy<TSelf, TComponent>(this IDummyComponent<TSelf, TComponent> dummyComponent, Func<TComponent, bool> breakableAction, bool throwIfFail = false)
        where TSelf : TComponent, IDummyComponent<TSelf, TComponent>
        where TComponent : BaseComponent
    {
        foreach (var building in dummyComponent.MMComponent.SelectedBuildings)
        {
            try
            {
                var comp = building.Prefab.GetComponentFast<TComponent>();
                if (comp)
                {
                    if (breakableAction(comp)) { break; }
                }
            }
            catch (Exception ex)
            {
                if (throwIfFail)
                {
                    throw;
                }
                else
                {
                    Debug.LogError(ex);
                }
            }
        }
    }

    public static TResult? ProxyFirstOrDefault<TSelf, TComponent, TResult>(this IDummyComponent<TSelf, TComponent> dummyComponent, Func<TComponent, TResult> func, bool throwIfFail = false)
        where TSelf : TComponent, IDummyComponent<TSelf, TComponent>
        where TComponent : BaseComponent
    {
        foreach (var building in dummyComponent.MMComponent.SelectedBuildings)
        {
            try
            {
                var comp = building.Prefab.GetComponentFast<TComponent>();
                if (comp)
                {
                    return func(comp);
                }
            }
            catch (Exception ex)
            {
                if (throwIfFail)
                {
                    throw;
                }
                else
                {
                    Debug.LogError(ex);
                }
            }
        }

        return default;
    }

    public static TResult? ProxyOriginalOrDefault<TSelf, TComponent, TResult>(this IDummyComponent<TSelf, TComponent> dummyComponent, Func<TComponent, TResult> func, bool throwIfFail = false)
        where TSelf : TComponent, IDummyComponent<TSelf, TComponent>
        where TComponent : BaseComponent
    {
        try
        {
            var comp = dummyComponent.MMComponent.Original.Prefab.GetComponentFast<TComponent>();
            if (comp)
            {
                return func(comp);
            }
        }
        catch (Exception ex)
        {
            if (throwIfFail)
            {
                throw;
            }
            else
            {
                Debug.LogError(ex);
            }
        }

        return default;
    }

    public static bool PatchBypass<TSelf, TComponent>(this TComponent dummyComponent) => dummyComponent is not TSelf;

    public static bool PatchRedirect<TSelf, TComponent>(this TComponent dummyComponent, Action<TSelf> action)
        where TSelf : TComponent, IDummyComponent<TSelf, TComponent>
        where TComponent : BaseComponent
    {
        if (dummyComponent is TSelf self)
        {
            action(self);
            return false;
        }

        return true;
    }

    public static bool PatchRedirect<TSelf, TComponent, TResult>(this TComponent dummyComponent, Func<TSelf, TResult> action, ref TResult __result)
        where TSelf : TComponent, IDummyComponent<TSelf, TComponent>
        where TComponent : BaseComponent
    {
        if (dummyComponent is TSelf self)
        {
            __result = action(self);
            return false;
        }

        return true;
    }

}