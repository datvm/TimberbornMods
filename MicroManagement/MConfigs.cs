global using MicroManagement.UI;

namespace MicroManagement;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this.BindFragments<EntityPanelFragmentProvider<BeaverManagementFragment>>();
        Bind<BeaverAssignmentTool>().AsSingleton();
    }
}
