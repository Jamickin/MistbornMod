using Terraria.ModLoader;

namespace MistbornMod
{
    public class MistbornMod : Mod
    {
        public static ModKeybind IronToggleHotkey { get; private set; }
        public static ModKeybind PewterToggleHotkey { get; private set; }
        public static ModKeybind TinToggleHotkey { get; private set; }
        public static ModKeybind SteelToggleHotkey { get; private set; }
        public static ModKeybind BrassToggleHotkey { get; private set; }
        public static ModKeybind ZincToggleHotkey { get; private set; }
        public static ModKeybind AtiumToggleHotkey { get; private set; }




        public override void Load()
        {
            // Register the hotkeys when the mod loads
            // The names ("Burn Iron", "Burn Pewter") are shown in the controls menu
            // The default keys ("F", "G") are just suggestions, users can rebind them
            IronToggleHotkey = KeybindLoader.RegisterKeybind(this, "Burn Iron", "F");
            PewterToggleHotkey = KeybindLoader.RegisterKeybind(this, "Burn Pewter", "G");
            TinToggleHotkey = KeybindLoader.RegisterKeybind(this, "Burn Tin", "H");
            SteelToggleHotkey = KeybindLoader.RegisterKeybind(this, "Burn Steel", "K");
            BrassToggleHotkey = KeybindLoader.RegisterKeybind(this, "Burn Brass", "K");
            ZincToggleHotkey = KeybindLoader.RegisterKeybind(this, "Burn Zinc", "K");
            AtiumToggleHotkey = KeybindLoader.RegisterKeybind(this, "Burn Atium", "K");

        }

        // It's good practice to unload static variables
        public override void Unload()
        {
            IronToggleHotkey = null;
            PewterToggleHotkey = null;
            SteelToggleHotkey = null;
            TinToggleHotkey = null;
            BrassToggleHotkey = null;
            ZincToggleHotkey = null;
        }
    }
}
