using Microsoft.Xna.Framework;
using MistbornMod.Common.Players;
using MistbornMod.Common.UI; // Add this using statement
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace MistbornMod.Common.Systems
{
    /// <summary>
    /// Handles rendering atmospheric effects during night time for Mistborn players
    /// </summary>
    public class MistRenderLayer : ModSystem
    {
        private static float mistIntensity = 0f;
        private const float baseMaxMistAlpha = 0.4f; // Base maximum opacity of mist
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
                        try
                        {
                            // Only draw if mist should be active
                            if (ShouldDrawMist())
                            {
                                DrawMistEffects();
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log the error but don't crash the game
                            ModContent.GetInstance<MistbornMod>()?.Logger.Error("Error in mist rendering: " + ex.Message);
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
            try
            {
                // Get config instance - FIXED: Use MetalUIConfig directly, not DraggableMetalUI.MetalUIConfig
                var config = ModContent.GetInstance<MetalUIConfig>();
                
                // Check if mist effects are enabled in config
                if (config != null && !config.EnableMistEffects)
                {
                    // Mist effects are disabled, fade out any existing mist
                    mistIntensity = Math.Max(0f, mistIntensity - MIST_FADE_SPEED * 2f);
                    return mistIntensity > 0f;
                }
                
                // Calculate max mist alpha based on config intensity
                float maxMistAlpha = baseMaxMistAlpha;
                if (config != null)
                {
                    maxMistAlpha = baseMaxMistAlpha * config.MistIntensity;
                }
                
                // Check if it's night time
                bool isNight = !Main.dayTime;
                
                // Skip rendering if it's daytime
                if (!isNight) 
                {
                    mistIntensity = Math.Max(0f, mistIntensity - MIST_FADE_SPEED);
                    return mistIntensity > 0f;
                }
                
                // Check if at least one player is Mistborn
                bool anyMistborn = false;
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (player != null && player.active)
                    {
                        var modPlayer = player.GetModPlayer<MistbornPlayer>();
                        if (modPlayer != null && modPlayer.IsMistborn)
                        {
                            anyMistborn = true;
                            break;
                        }
                    }
                }
                
                // Adjust mist intensity based on whether we should show it
                if (anyMistborn)
                {
                    // Fade in the mist
                    mistIntensity = Math.Min(maxMistAlpha, mistIntensity + MIST_FADE_SPEED);
                    return true;
                }
                else
                {
                    // Fade out mist if it's currently visible
                    if (mistIntensity > 0f)
                    {
                        mistIntensity = Math.Max(0f, mistIntensity - MIST_FADE_SPEED);
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Log error and disable mist for this frame
                ModContent.GetInstance<MistbornMod>()?.Logger.Error("Error checking mist conditions: " + ex.Message);
                return false;
            }
        }
        
        /// <summary>
        /// Draw mist particle effects on the screen
        /// </summary>
        private void DrawMistEffects()
        {
            try
            {
                // Get config for particle frequency safely - FIXED: Use MetalUIConfig directly
                var config = ModContent.GetInstance<MetalUIConfig>();
                float particleFrequency = 1.0f;
                float intensityMultiplier = 1.0f;
                
                if (config != null)
                {
                    particleFrequency = config.MistParticleFrequency;
                    intensityMultiplier = config.MistIntensity;
                }
                
                // Adjust spawn rate based on config
                int baseSpawnRate = 40;
                int adjustedSpawnRate = Math.Max(10, (int)(baseSpawnRate / particleFrequency));
                
                // Occasionally spawn mist particles for a more dynamic effect
                if (Main.rand.NextBool(adjustedSpawnRate) && mistIntensity > 0.1f)
                {
                    SpawnMistParticle(intensityMultiplier);
                }
            }
            catch (Exception ex)
            {
                // Log error but continue
                ModContent.GetInstance<MistbornMod>()?.Logger.Error("Error drawing mist effects: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Spawn a mist particle at a random screen position
        /// </summary>
        private void SpawnMistParticle(float intensityMultiplier = 1.0f)
        {
            try
            {
                // Calculate a random position on screen
                int x = Main.rand.Next(0, Main.screenWidth);
                int y = Main.rand.Next(0, Main.screenHeight);
                
                // Convert screen position to world position
                Vector2 worldPos = Main.screenPosition + new Vector2(x, y);
                
                // Adjust particle properties based on intensity
                float baseScale = Main.rand.NextFloat(1.5f, 2.5f);
                float adjustedScale = baseScale * intensityMultiplier;
                
                Color baseColor = new Color(200, 220, 255) * 0.7f;
                Color adjustedColor = baseColor * intensityMultiplier;
                
                // Create a dust particle with mist-like properties
                Dust dust = Dust.NewDustPerfect(
                    worldPos,
                    DustID.Cloud,
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    100,
                    adjustedColor,
                    adjustedScale
                );
                
                if (dust != null)
                {
                    dust.noGravity = true;
                    dust.noLight = true;
                    dust.fadeIn = 1.2f * intensityMultiplier;
                }
            }
            catch (Exception ex)
            {
                // Log error but don't crash
                ModContent.GetInstance<MistbornMod>()?.Logger.Error("Error spawning mist particle: " + ex.Message);
            }
        }
    }
}