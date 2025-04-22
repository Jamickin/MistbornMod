using Microsoft.Xna.Framework;
using MistbornMod.Common.Players;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace MistbornMod
{
    /// <summary>
    /// Handles rendering atmospheric effects during night time for Mistborn players
    /// </summary>
    public class MistRenderLayer : ModSystem
    {
        private static float mistIntensity = 0f;
        private const float MAX_MIST_ALPHA = 0.4f; // Maximum opacity of mist
        private const float MIST_FADE_SPEED = 0.01f; // Speed at which mist fades in/out
        
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
                            DrawMistEffects();
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
        
        /// <summary>
        /// Check if mist effects should be drawn (night time and at least one Mistborn player)
        /// </summary>
        private bool ShouldDrawMist()
        {
            // Check if it's night time
            bool isNight = !Main.dayTime;
            
            // Skip rendering if it's daytime
            if (!isNight) 
            {
                mistIntensity = Math.Max(0f, mistIntensity - MIST_FADE_SPEED);
                return mistIntensity > 0f; // Still draw while fading out
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
            
            // Adjust mist intensity based on whether we should show it
            if (anyMistborn)
            {
                // Fade in the mist
                mistIntensity = Math.Min(MAX_MIST_ALPHA, mistIntensity + MIST_FADE_SPEED);
                return true;
            }
            else
            {
                // Fade out mist if it's currently visible
                if (mistIntensity > 0f)
                {
                    mistIntensity = Math.Max(0f, mistIntensity - MIST_FADE_SPEED);
                    return true; // Still draw while fading out
                }
                return false;
            }
        }
        
        /// <summary>
        /// Draw mist particle effects on the screen
        /// </summary>
        private void DrawMistEffects()
        {
            // Occasionally spawn mist particles for a more dynamic effect
            if (Main.rand.NextBool(40) && mistIntensity > 0.1f)
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