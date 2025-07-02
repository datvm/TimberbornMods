global using MicroManagement.Services;
global using MicroManagement.UI;

namespace MicroManagement;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<BeaverAssignmentTool>()
            .BindSingleton<BeaverAssignmentService>()

            .BindFragment<BeaverManagementFragment>()
            .BindFragment<SendAllToDistrictCenterFragment>()
        ;
    }
}
