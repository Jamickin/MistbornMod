using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace MistbornMod
{
    /// <summary>
    /// Handles rendering the mist overlay during night time for Mistborn players
    /// </summary>
    public class MistRenderLayer : ModSystem
    {
        private static Texture2D mistTexture;
        private static float mistAlpha = 0f;
        private static float mistIntensity = 0f;
        private const float MAX_MIST_ALPHA = 0.4f; // Maximum opacity of mist
        private const float MIST_FADE_SPEED = 0.01f; // Speed at which mist fades in/out
        
        public override void Load()
        {
            if (!Main.dedServ) // Skip loading on dedicated server
            {
                // Create a dynamic texture for mist if needed
                // Otherwise, load your mist texture here
                mistTexture = ModContent.Request<Texture2D>("MistbornMod/Assets/Textures/Mist").Value;
            }
        }
        
        public override void Unload()
        {
            mistTexture = null;
        }
        
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            // Find the index of the vanilla hotbar layer (to place our UI above it)
            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Interface Logic 1"));
            
            if (inventoryIndex != -1)
            {
                // Insert our mist rendering just before the inventory
                layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(
                    "MistbornMod: Mist Effect",
                    delegate
                    {
                        // Only draw if mist should be active
                        if (ShouldDrawMist())
                        {
                            DrawMistOverlay();
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
        
        /// <summary>
        /// Check if mist should be drawn (night time and at least one Mistborn player)
        /// </summary>
        private bool ShouldDrawMist()
        {
            // Check if it's night time
            bool isNight = !Main.dayTime;
            
            // Skip rendering if it's daytime - the mist only appears at night
            if (!isNight) 
            {
                // Fade out mist if it's currently visible
                if (mistAlpha > 0f)
                {
                    mistAlpha = Math.Max(0f, mistAlpha - MIST_FADE_SPEED);
                }
                return mistAlpha > 0f; // Still draw while fading out
            }
            
            // Check if at least one player is Mistborn
            bool anyMistborn = false;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active && player.GetModPlayer<MistbornPlayer>().IsMistborn)
                {
                    anyMistborn = true;
                    break;
                }
            }
            
            // Adjust mist alpha based on whether we should show it
            if (anyMistborn)
            {
                // Fade in the mist
                mistAlpha = Math.Min(MAX_MIST_ALPHA, mistAlpha + MIST_FADE_SPEED);
                
                // Calculate intensity based on time and position
                float timeIntensity = (float)Math.Sin(Main.GameUpdateCount * 0.01f) * 0.1f + 0.9f;
                mistIntensity = timeIntensity * 0.2f + 0.8f;
                
                return true;
            }
            else
            {
                // Fade out mist if it's currently visible
                if (mistAlpha > 0f)
                {
                    mistAlpha = Math.Max(0f, mistAlpha - MIST_FADE_SPEED);
                    return true; // Still draw while fading out
                }
                return false;
            }
        }
        
        /// <summary>
        /// Draw the mist overlay on the screen
        /// </summary>
        private void DrawMistOverlay()
        {
            if (mistTexture == null) return;
            
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Begin drawing with additive blend mode for a ghostly effect
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone);
            
            // Get screen dimensions
            Rectangle screen = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
            
            // Calculate scrolling offsets for parallax effect
            float offsetX = -Main.screenPosition.X * 0.1f % mistTexture.Width;
            float offsetY = -Main.screenPosition.Y * 0.05f % mistTexture.Height;
            
            // Add time-based movement to make the mist flow
            offsetX += (float)Math.Sin(Main.GameUpdateCount * 0.01f) * 2f;
            offsetY += (float)Math.Cos(Main.GameUpdateCount * 0.008f) * 1.5f;
            
            // Create a tiling region for the mist texture
            Rectangle sourceRect = new Rectangle(
                (int)offsetX, 
                (int)offsetY, 
                mistTexture.Width * 2, 
                mistTexture.Height * 2
            );
            
            // Calculate the final opacity
            Color mistColor = new Color(180, 200, 220, 255) * (mistAlpha * mistIntensity);
            
            // Draw the mist layer
            spriteBatch.Draw(mistTexture, screen, sourceRect, mistColor);
            
            // End the sprite batch
            spriteBatch.End();
            
            // Occasionally spawn mist particles for a more dynamic effect
            if (Main.rand.NextBool(40) && mistAlpha > 0.1f)
            {
                SpawnMistParticle();
            }
        }
        
        /// <summary>
        /// Spawn a mist particle at a random screen position
        /// </summary>
        private void SpawnMistParticle()
        {
            // Calculate a random position on screen
            int x = Main.rand.Next(0, Main.screenWidth);
            int y = Main.rand.Next(0, Main.screenHeight);
            
            // Convert screen position to world position
            Vector2 worldPos = Main.screenPosition + new Vector2(x, y);
            
            // Create a dust particle with mist-like properties
            Dust dust = Dust.NewDustPerfect(
                worldPos,
                DustID.Cloud,
                Main.rand.NextVector2Circular(0.5f, 0.5f),
                100,
                new Color(200, 220, 255) * 0.7f,
                Main.rand.NextFloat(1.5f, 2.5f)
            );
            
            dust.noGravity = true;
            dust.noLight = true;
            dust.fadeIn = 1.2f;
        }
    }
}