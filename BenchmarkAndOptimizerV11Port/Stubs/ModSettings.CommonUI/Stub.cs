using ModSettings.Core; using ModSettings.CoreUI; using UnityEngine.UIElements;
namespace ModSettings.CommonUI
{
    public sealed class ModSettingElement : IModSettingElement
    {
        public ModSettingElement(VisualElement root, ModSetting modSetting) { Root=root; ModSetting=modSetting; }
        public VisualElement Root { get; }
        public ModSetting ModSetting { get; }
        public bool ShouldBlockInput => false;
    }
}
