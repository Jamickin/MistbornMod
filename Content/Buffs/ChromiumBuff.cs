using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using MistbornMod.Common.Players;

namespace MistbornMod.Content.Buffs
{
    public class ChromiumBuff : MetalBuff
    {
        // Radius around the player that affects other players in multiplayer
        private const float ChromiumEffectRange = 200f;
        
        // Cooldown system to prevent constant draining
        private int effectCooldown = 0;
        private const int BaseCooldown = 30; // Half-second cooldown between wipes

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Metal = MetalType.Chromium;
            
            // Set description in language files, not here
            // Display name will be set in localization file
        }

public override void ApplyBuffEffect(Player player, bool isFlaring)
        {
            // Get the MistbornPlayer instance to check flaring status
            MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>();
            
            // Decrease cooldown if active
            if (effectCooldown > 0)
            {
                effectCooldown--;
            }
            
            // Only activate when player is actively using Chromium (not just when buff is present)
            if (modPlayer.IsActivelyChromiumStripping && effectCooldown <= 0)
            {
                // Drain a bit of Chromium itself too
                if (modPlayer.MetalReserves.TryGetValue(MetalType.Chromium, out int chromiumReserve))
                {
                    // Consume Chromium at a reasonable rate
                    int consumption = modPlayer.IsFlaring ? 10 : 5; // Consumes faster when flaring
                    modPlayer.MetalReserves[MetalType.Chromium] = System.Math.Max(0, chromiumReserve - consumption);
                }
                
                // Only continue if we still have Chromium reserves
                if (modPlayer.MetalReserves.TryGetValue(MetalType.Chromium, out int remainingChromium) && remainingChromium > 0)
                {
                    // Clear all metal reserves except Chromium itself
                    foreach (MetalType metal in System.Enum.GetValues(typeof(MetalType)))
                    {
                        // Skip Chromium since we don't want to clear our own metal
                        if (metal != MetalType.Chromium && modPlayer.MetalReserves.TryGetValue(metal, out int _))
                        {
                            // Set reserves to 0
                            modPlayer.MetalReserves[metal] = 0;
                            
                            // Turn off burning for this metal
                            if (modPlayer.BurningMetals.TryGetValue(metal, out bool burning) && burning)
                            {
                                modPlayer.BurningMetals[metal] = false;
                                
                                // Clear related buffs
                                int buffId = modPlayer.GetBuffIDForMetal(metal);
                                if (buffId != -1 && player.HasBuff(buffId))
                                {
                                    player.ClearBuff(buffId);
                                }
                            }
                        }
                    }
                    
                    // Trigger a visual effect to show the metal stripping
                    CreateChromiumEffect(player, modPlayer.IsFlaring);
                    
                    // Show message to player
                    Main.NewText("Your metallic reserves have been stripped away!", 220, 220, 255);
                    
                    // Set cooldown to prevent constant spam
                    effectCooldown = modPlayer.IsFlaring ? BaseCooldown / 2 : BaseCooldown;
                }
            }
            
            // Visual effects for active burning
            if (Main.rand.NextBool(modPlayer.IsFlaring ? 4 : 8))
            {
                Color dustColor = modPlayer.IsFlaring ? new Color(150, 230, 255) : new Color(120, 180, 220);
                Dust.NewDust(player.position, player.width, player.height, DustID.BlueCrystalShard, 0, 0, 150, dustColor, modPlayer.IsFlaring ? 1.2f : 0.8f);
            }
        }
        
        private void CreateChromiumEffect(Player player, bool flaring)
        {
            // Create a burst of dust particles to visualize the metal-stripping effect
            int dustCount = flaring ? 40 : 20;
            float scale = flaring ? 1.5f : 1.0f;
            
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 velocity = Main.rand.NextVector2CircularEdge(4f * scale, 4f * scale);
                Dust d = Dust.NewDustPerfect(
                    player.Center, 
                    DustID.BlueCrystalShard, 
                    velocity, 
                    100, 
                    new Color(150, 230, 255), 
                    flaring ? 1.5f : 1.0f
                );
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }
            
            // Add a flash effect at the player's position
            if (flaring)
            {
                // Create a bright flash when flaring
                for (int i = 0; i < 10; i++)
                {
                    Dust d = Dust.NewDustPerfect(
                        player.Center, 
                        DustID.Electric, 
                        Main.rand.NextVector2Circular(3f, 3f), 
                        0, 
                        new Color(200, 240, 255), 
                        2.0f
                    );
                    d.noGravity = true;
                }
            }
        }
        
        public override void OnBuffEnd(Player player, MistbornPlayer modPlayer)
        {
            // Reset cooldown when buff ends
            effectCooldown = 0;
            
            // Reset the active flag
            modPlayer.IsActivelyChromiumStripping = false;
            
            // Show message
            Main.NewText("Chromium effect has worn off.", 180, 180, 220);
        }
    }
}