namespace TimberUi.CommonUi;

public class BottomBarModuleProvider<T>(T btn) : IProvider<BottomBarModule> where T : IBottomBarElementProvider
{
    public BottomBarModule Get()
    {
        BottomBarModule.Builder builder = new();
        builder.AddRightSectionElement(btn);

        return builder.Build();
    }
}
