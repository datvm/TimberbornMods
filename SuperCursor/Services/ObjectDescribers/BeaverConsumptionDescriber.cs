using System;
using System.Collections.Generic;
using System.Text;

namespace SuperCursor.Services.ObjectDescribers
{
    [ServicePriority(25)]   
    public class BeaverConsumptionDescriber(
    ILoc loc
) : IObjectDescriber
    {
        public void Describe(StringBuilder builder, BaseComponent component)
        {
            if (component is not Beaver beaver)
                return;

            
            if (beaver.TryGetComponent(out Thirst thirst))
            {
                var waterPerDay = thirst.ConsumptionRate;  
                builder.AppendLine(loc.T("LV.SC.BeaverWaterConsumption", waterPerDay));
            }

            if (beaver.TryGetComponent(out Hunger hunger))
            {
                var foodPerDay = hunger.ConsumptionRate;    
                builder.AppendLine(loc.T("LV.SC.BeaverFoodConsumption", foodPerDay));
            }
        }
    }
}
