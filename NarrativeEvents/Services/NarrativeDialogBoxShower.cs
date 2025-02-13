global using Timberborn.CoreUI;

namespace NarrativeEvents.Services;

public class NarrativeDialogBoxShower(DialogBoxShower diag)
{

    
    public class NarrativeDialogBoxBuilder(DialogBoxShower.Builder builder)
    {


        public void Show()
        {
            builder.Show();
        }

    }

    public NarrativeDialogBoxBuilder Create()
    {
        var builder = diag.Create();

        return new(builder);
    }

}
