using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using MistbornMod.Common.Players;

namespace MistbornMod.Content.Buffs
{
    public class TinBuff : MetalBuff
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Metal = MetalType.Tin;
        }

public override void ApplyBuffEffect(Player player, bool isFlaring)
        {
            // Get the MistbornPlayer instance to check flaring status
            MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>();
            
            // Basic abilities always on
            player.nightVision = true;
            player.detectCreature = true;
            player.dangerSense = true;
            
            // Crit chance affected by flaring
            float multiplier = modPlayer.IsFlaring ? 2.0f : 1.0f;
            player.GetCritChance(DamageClass.Generic) += 15 * multiplier;
            
            // When flaring, add additional effects
            if (modPlayer.IsFlaring)
            {
                // Enhanced light radius when flaring
                Lighting.AddLight(player.Center, 0.5f, 0.5f, 0.7f);
                
                // Enhanced detection abilities when flaring
                player.findTreasure = true; // Spelunker effect
                player.biomeSight = true; // Sense dangerous biomes
                
                // Visual effect for flaring tin
                if (Main.rand.NextBool(10))
                {
                    Dust.NewDust(
                        player.position,
                        player.width,
                        player.height,
                        DustID.MagicMirror,
                        0f, -1f,
                        150,
                        default,
                        0.8f
                    );
                }
            }
        }
    }
}