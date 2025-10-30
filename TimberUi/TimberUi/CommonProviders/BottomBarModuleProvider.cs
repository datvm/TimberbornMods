namespace TimberUi.CommonProviders;

public class BottomBarModuleProvider<T>(T btn) : IProvider<BottomBarModule> where T : IBottomBarElementsProvider
{
    public BottomBarModule Get()
    {
        BottomBarModule.Builder builder = new();
        builder.AddRightSectionElement(btn);

        return builder.Build();
    }
}
