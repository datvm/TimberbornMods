using Timberborn.BlockSystem;
using Timberborn.WaterSourceSystem;

namespace TheArchitectsToolkit.Services;

public class ToolkitMapEditorService(EventBus e) //: ILoadableSingleton, IUnloadableSingleton
{
    //public void Load()
    //{
    //    e.Register(this);
    //}

    //public void Unload()
    //{
    //    e.Unregister(this);
    //}

    //[OnEvent]
    //public void OnBlockObjectFinished(EnteredFinishedStateEvent e)
    //{
    //    var ws = e.BlockObject.GetComponentFast<WaterSource>();
    //    if (ws)
    //    {
    //        Debug.Log($"WS entered finished state");

    //        ws.SpecifiedStrength = MSettings.DefaultWaterSourceStrength;
    //    }
    //}

}
