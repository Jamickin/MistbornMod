using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace MistbornMod.UI
{
    // This class handles the UI system for the Mistborn mod
    internal class MistbornUISystem : ModSystem
    {
        // UI states 
        internal MetalReservesUI MetalReservesUI;
        
        // UI layers
        private UserInterface _metalReservesInterface;
        
        // UI resource management
        internal static Asset<Texture2D> MetalIconTexture;
        internal static Asset<Texture2D> MetalBarTexture;
        internal static Asset<Texture2D> MetalUIBackground;
        
        // Dictionary to store metal colors
        internal static Dictionary<MetalType, Color> MetalColors = new Dictionary<MetalType, Color>();

        public override void Load()
        {
            // Initialize UI elements if not in server mode
            if (!Main.dedServ)
            {
                // Load textures
                MetalIconTexture = ModContent.Request<Texture2D>("MistbornMod/UI/MetalIcons");
                MetalBarTexture = ModContent.Request<Texture2D>("MistbornMod/UI/MetalBar");
                MetalUIBackground = ModContent.Request<Texture2D>("MistbornMod/UI/UIBackground");
                
                // Initialize metal colors
                InitializeMetalColors();
                
                // Create UI state instances
                MetalReservesUI = new MetalReservesUI();
                MetalReservesUI.Activate();
                
                // Initialize user interfaces
                _metalReservesInterface = new UserInterface();
                _metalReservesInterface.SetState(MetalReservesUI);
            }
        }

        public override void Unload()
        {
            // Unload textures and references
            MetalIconTexture = null;
            MetalBarTexture = null;
            MetalUIBackground = null;
            MetalColors.Clear();
        }
        
        private void InitializeMetalColors()
        {
            // Define colors for each metal type
            MetalColors[MetalType.Iron] = new Color(100, 120, 140);     // Blueish-gray
            MetalColors[MetalType.Steel] = new Color(180, 190, 210);    // Light steel blue
            MetalColors[MetalType.Tin] = new Color(180, 180, 190);      // Light gray
            MetalColors[MetalType.Pewter] = new Color(120, 120, 130);   // Dark gray
            MetalColors[MetalType.Brass] = new Color(190, 150, 80);     // Brass/bronze
            MetalColors[MetalType.Zinc] = new Color(210, 210, 230);     // Light blue-white
            MetalColors[MetalType.Copper] = new Color(190, 110, 50);    // Copper
            MetalColors[MetalType.Bronze] = new Color(170, 120, 60);    // Bronze
            MetalColors[MetalType.Atium] = new Color(255, 255, 255);    // White/silver
        }

        public override void UpdateUI(GameTime gameTime)
        {
            // Only update if the player exists and has a character
            if (Main.gameMenu || Main.LocalPlayer == null || Main.LocalPlayer.dead) return;
            
            // Get the MistbornPlayer instance
            MistbornPlayer modPlayer = Main.LocalPlayer.GetModPlayer<MistbornPlayer>();
            
            // Only update the interface if the UI should be visible
            if (modPlayer.ShowMetalUI)
            {
                _metalReservesInterface?.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            // Find the index of the vanilla hotbar layer (to place our UI above it)
            int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
            if (resourceBarIndex != -1)
            {
                // Insert our UI at the proper location
                layers.Insert(resourceBarIndex + 1, new LegacyGameInterfaceLayer(
                    "MistbornMod: Metal Reserves UI",
                    delegate
                    {
                        // Only draw if the player exists and has a character
                        if (Main.gameMenu || Main.LocalPlayer == null || Main.LocalPlayer.dead) return true;
                        
                        // Get the MistbornPlayer instance
                        MistbornPlayer modPlayer = Main.LocalPlayer.GetModPlayer<MistbornPlayer>();
                        
                        // Only draw the interface if the UI should be visible
                        if (modPlayer.ShowMetalUI)
                        {
                            _metalReservesInterface?.Draw(Main.spriteBatch, new GameTime());
                        }
                        
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}