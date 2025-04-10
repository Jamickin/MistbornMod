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
        public static ModKeybind ChromiumToggleHotkey { get; private set; } // Added Chromium hotkey
        public static ModKeybind FlareToggleHotkey { get; private set; }
        public static ModKeybind CopperToggleHotkey { get; private set; }
        public static ModKeybind BronzeToggleHotkey { get; private set; }   


        public override void Load()
        {
            // Register the hotkeys when the mod loads
            // The names ("Burn Iron", "Burn Pewter") are shown in the controls menu
            // Note: For Iron and Steel we explain they need to be held
            IronToggleHotkey = KeybindLoader.RegisterKeybind(this, "Burn Iron", "F");
            PewterToggleHotkey = KeybindLoader.RegisterKeybind(this, "Burn Pewter", "G");
            TinToggleHotkey = KeybindLoader.RegisterKeybind(this, "Burn Tin", "H");
            SteelToggleHotkey = KeybindLoader.RegisterKeybind(this, "Burn Steel", "J");
            BrassToggleHotkey = KeybindLoader.RegisterKeybind(this, "Burn Brass", "B");
            ZincToggleHotkey = KeybindLoader.RegisterKeybind(this, "Burn Zinc", "Z");
            AtiumToggleHotkey = KeybindLoader.RegisterKeybind(this, "Burn Atium", "V");
            ChromiumToggleHotkey = KeybindLoader.RegisterKeybind(this, "Burn Chromium", "K"); // Added Chromium with default key K
            CopperToggleHotkey = KeybindLoader.RegisterKeybind(this, "Burn Copper", "C");
            BronzeToggleHotkey = KeybindLoader.RegisterKeybind(this, "Burn Bronze", "N");
            // Add the flare toggle keybind - use Alt as default since it's commonly available
            FlareToggleHotkey = KeybindLoader.RegisterKeybind(this, "Flare Metals", "LeftAlt");
            
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
            AtiumToggleHotkey = null;
            ChromiumToggleHotkey = null; // Added Chromium unload
            FlareToggleHotkey = null;
            CopperToggleHotkey = null;
            BronzeToggleHotkey = null;
        }
    }
}