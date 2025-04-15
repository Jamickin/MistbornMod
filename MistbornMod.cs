using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.Config;
using Terraria;


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
        public static ModKeybind ChromiumToggleHotkey { get; private set; }
        public static ModKeybind FlareToggleHotkey { get; private set; }
        public static ModKeybind CopperToggleHotkey { get; private set; }
        public static ModKeybind BronzeToggleHotkey { get; private set; }
        // Add the new metal detection hotkey
        public static ModKeybind MetalDetectionHotkey { get; private set; }
        
        // Helper method for creating zinc stalagmites - accessible from the cleanup pass
        public static void CreateZincStalagmite(int centerX, int bottomY, int height, int baseWidth)
        {
            // Increased embed depth to start deeper in the ground
            int embedDepth = Terraria.WorldGen.genRand.Next(6, 12);  // 6-12 blocks deep
            
            // Start from bottom and work upward
            for (int y = 0; y < height + embedDepth; y++)  // Add embedDepth to total height
            {
                // Calculate current Y position (starting deeper below surface, then going up)
                int currentY = bottomY - y;
                
                // Calculate tapering width at this height
                // Width decreases as we go up, with some randomness
                float heightRatio = 1.0f - ((float)(y - embedDepth) / height);
                if (y < embedDepth) heightRatio = 1.0f;  // Full width while underground
                
                int currentWidth = System.Math.Max(1, (int)(baseWidth * heightRatio * heightRatio));
                
                // Add some randomness to width for a more natural look
                currentWidth += Terraria.WorldGen.genRand.Next(-1, 2);
                
                // Fill width at this height
                for (int w = -currentWidth / 2; w <= currentWidth / 2; w++)
                {
                    // Calculate actual X position
                    int currentX = centerX + w;
                    
                    // Skip if out of bounds
                    if (currentX < 5 || currentX >= Terraria.Main.maxTilesX - 5 || 
                        currentY < 5 || currentY >= Terraria.Main.maxTilesY - 5)
                        continue;
                    
                    Terraria.Tile tile = Terraria.Framing.GetTileSafely(currentX, currentY);
                    
                    // Force-replace any existing tiles to ensure our zinc isn't overwritten
                    // For underground portions, only replace if not dangerous tiles (like hellstone)
                    bool canReplace = true;
                    
                    if (tile.HasTile)
                    {
                        // Don't replace some special tiles even underground
                        if (tile.TileType == Terraria.ID.TileID.Hellstone || 
                            tile.TileType == Terraria.ID.TileID.Containers ||
                            tile.TileType == Terraria.ID.TileID.Containers2)
                        {
                            canReplace = false;
                        }
                    }
                    
                    if (canReplace)
                    {
                        // Clear any existing tile
                        if (tile.HasTile)
                        {
                            Terraria.WorldGen.KillTile(currentX, currentY, false, false, true);
                        }
                        
                        // Place zinc with 80% probability for some texture
                        if (Terraria.WorldGen.genRand.NextFloat() < 0.8f)
                        {
                            // Use forcePlacement=true to ensure it's placed regardless of existing tiles
                            Terraria.WorldGen.PlaceTile(currentX, currentY, 
                                ModContent.TileType<Tiles.ZincOreTile>(), true, true);
                            
                            // Set important flags to prevent overwriting
                            tile = Terraria.Framing.GetTileSafely(currentX, currentY);
                            
                            // These flags help protect the tile from being overwritten
                            tile.HasTile = true;
                            tile.TileType = (ushort)ModContent.TileType<Tiles.ZincOreTile>();
                        }
                    }
                }
            }
            
            // Add some scattered small ore nodes around the base
            for (int i = 0; i < baseWidth * 2; i++)
            {
                int offsetX = Terraria.WorldGen.genRand.Next(-baseWidth * 2, baseWidth * 2 + 1);
                int offsetY = Terraria.WorldGen.genRand.Next(-5, 7);
                int scatterX = centerX + offsetX;
                int scatterY = bottomY + offsetY;
                
                // Skip if out of bounds
                if (scatterX < 5 || scatterX >= Terraria.Main.maxTilesX - 5 || 
                    scatterY < 5 || scatterY >= Terraria.Main.maxTilesY - 5)
                    continue;
                
                Terraria.Tile tile = Terraria.Framing.GetTileSafely(scatterX, scatterY);
                
                // Only place in solid tiles (that aren't already zinc)
                if (tile.HasTile && Terraria.Main.tileSolid[tile.TileType] && 
                    tile.TileType != ModContent.TileType<Tiles.ZincOreTile>())
                {
                    // Create small ore cluster
                    Terraria.WorldGen.TileRunner(
                        scatterX, scatterY,
                        Terraria.WorldGen.genRand.Next(2, 5), // Small size
                        Terraria.WorldGen.genRand.Next(2, 6), // Small steps
                        ModContent.TileType<Tiles.ZincOreTile>(),
                        false, 0f, 0f, false, true);
                }
            }
        }
        
        // public void SaveConfig(ModConfig config)
        // {
        //     // This is the correct way to save a ModConfig in tModLoader
        //     if (config != null)
        //     {
        //         // Force a config save
        //         Configuration.Save(config);
        //     }
        // }
        
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
            ChromiumToggleHotkey = KeybindLoader.RegisterKeybind(this, "Burn Chromium", "K");
            CopperToggleHotkey = KeybindLoader.RegisterKeybind(this, "Burn Copper", "C");
            BronzeToggleHotkey = KeybindLoader.RegisterKeybind(this, "Burn Bronze", "N");
            // Add the flare toggle keybind
            FlareToggleHotkey = KeybindLoader.RegisterKeybind(this, "Flare Metals", "LeftAlt");
            UI.DraggableMetalUI.ToggleUIHotkey = KeybindLoader.RegisterKeybind(this, "Toggle Metal UI", "M");

            // Add the new metal detection hotkey (using LeftShift as default)
            MetalDetectionHotkey = KeybindLoader.RegisterKeybind(this, "Detect Metals", "X");
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
            ChromiumToggleHotkey = null;
            FlareToggleHotkey = null;
            CopperToggleHotkey = null;
            BronzeToggleHotkey = null;
            MetalDetectionHotkey = null; // Unload the new hotkey
        }
    }
}