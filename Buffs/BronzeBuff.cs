// Buffs/BronzeBuff.cs
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Collections.Generic;

namespace MistbornMod.Buffs
{
    public class BronzeBuff : MetalBuff
    {
        private const float BaseScanRange = 1200f; // Base detection range
        
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Metal = MetalType.Bronze;
        }
        
        public override void Update(Player player, ref int buffIndex)
        {
            // Get the MistbornPlayer instance to check flaring status
            MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>();
            float multiplier = modPlayer.IsFlaring ? 2.0f : 1.0f;
            
            float currentScanRange = BaseScanRange * multiplier;
            modPlayer.IsBronzeScanning = true;
            
            // In multiplayer, scan for other allomancers
            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player targetPlayer = Main.player[i];
                    if (!targetPlayer.active || targetPlayer == player) continue;
                    
                    MistbornPlayer targetModPlayer = targetPlayer.GetModPlayer<MistbornPlayer>();
                    
                    // Skip if target is in a coppercloud
                    if (targetModPlayer.IsHiddenByCoppercloud) continue;
                    
                    // Check if target is burning any metals
                    bool isTargetBurning = targetModPlayer.BurningMetals.Any(m => m.Value) || 
                                          targetModPlayer.IsActivelySteelPushing || 
                                          targetModPlayer.IsActivelyIronPulling ||
                                          targetModPlayer.IsActivelyChromiumStripping;
                    
                    if (isTargetBurning)
                    {
                        float distSq = Vector2.DistanceSquared(player.Center, targetPlayer.Center);
                        if (distSq < currentScanRange * currentScanRange)
                        {
                            // Show a line to the detected allomancer
                            DrawLineWithDust(player.Center, targetPlayer.Center, DustID.Copper, 0.05f);
                            
                            // Show message if rarely 
                            if (Main.rand.NextBool(120)) // Every ~2 seconds
                            {
                                string message = $"Detected {targetPlayer.name} using Allomancy!";
                                Main.NewText(message, new Color(180, 130, 80));
                            }
                        }
                    }
                }
            }
            
            // Visual effects
            if (Main.rand.NextBool(modPlayer.IsFlaring ? 10 : 15))
            {
                Dust.NewDust(
                    player.position,
                    player.width,
                    player.height,
                    DustID.Copper,
                    0f, 0f,
                    150, 
                    default,
                    modPlayer.IsFlaring ? 0.8f : 0.5f
                );
            }
        }
        
        private void DrawLineWithDust(Vector2 start, Vector2 end, int dustType, float density = 0.1f)
        {
            // Similar to other metals' line drawing code
            if (Vector2.DistanceSquared(start, end) < 16f * 16f) return;
            
            Vector2 direction = end - start;
            float distance = direction.Length();
            if (distance == 0f) return;
            
            direction.Normalize();
            int steps = (int)(distance * density);
            if (steps <= 0) return;
            
            // Get the player's flaring status for dust intensity
            MistbornPlayer modPlayer = Main.LocalPlayer.GetModPlayer<MistbornPlayer>();
            bool isFlaring = modPlayer?.IsFlaring ?? false;
            
            for (int i = 1; i <= steps; i++)
            {
                float progress = (float)i / steps;
                Vector2 dustPos = start + direction * distance * progress;
                if(Main.rand.NextBool(isFlaring ? 3 : 5))
                {
                    Dust dust = Dust.NewDustPerfect(dustPos, dustType, Vector2.Zero, 150, default, isFlaring ? 0.4f : 0.3f);
                    dust.noGravity = true;
                    dust.velocity *= 0.1f;
                    dust.fadeIn = 0.5f;
                }
            }
        }
        
        public override void OnBuffEnd(Player player, MistbornPlayer modPlayer)
        {
            modPlayer.IsBronzeScanning = false;
        }
    }
}