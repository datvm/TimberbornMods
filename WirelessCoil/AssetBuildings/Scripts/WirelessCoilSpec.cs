using Timberborn.BaseComponentSystem;
using UnityEngine;

namespace WirelessCoil
{
    public class WirelessCoilSpec : BaseComponent
    {

        [SerializeField]
        int range;

        public int Range => range;
    }

}