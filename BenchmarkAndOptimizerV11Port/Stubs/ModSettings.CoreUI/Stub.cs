using ModSettings.Core;
using Timberborn.Modding;
using UnityEngine.UIElements;
namespace ModSettings.CoreUI
{
    public interface IModSettingElement { VisualElement Root { get; } ModSetting ModSetting { get; } bool ShouldBlockInput { get; } }
    public interface IModSettingElementFactory { int Priority { get; } bool TryCreateElement(ModSetting modSetting, out IModSettingElement? element); }
    public class ModSettingsBox
    {
        public void Open(Mod mod) { }
        public VisualElement GetPanel() => new VisualElement();
    }
}
