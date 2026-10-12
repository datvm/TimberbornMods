using System;
using Timberborn.Modding;
using Timberborn.SettingsSystem;
namespace ModSettings.Core
{
    [Flags] public enum ModSettingsContext { None=0, MainMenu=1, Game=2, MapEditor=4, All=7 }
    public sealed class ModSettingDescriptor
    {
        public static ModSettingDescriptor Create(string name)=>new ModSettingDescriptor();
    }
    public abstract class ModSetting
    {
        protected ModSetting(ModSettingDescriptor descriptor) { }
        public abstract void Reset();
    }
    public abstract class NonPersistentSetting : ModSetting
    {
        protected NonPersistentSetting(ModSettingDescriptor descriptor):base(descriptor) { }
    }
    public sealed class ModSettingsOwnerRegistry { }
    public abstract class ModSettingsOwner
    {
        protected ModSettingsOwner(ISettings settings, ModSettingsOwnerRegistry registry, ModRepository repository) { }
        public abstract string ModId { get; }
        public virtual ModSettingsContext ChangeableOn => ModSettingsContext.MainMenu;
    }
}
