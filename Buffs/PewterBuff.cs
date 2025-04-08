using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace MistbornMod.Buffs
{
    public class PewterBuff : MetalBuff
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Metal = MetalType.Pewter; 
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // Get the MistbornPlayer instance to check flaring status
            MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>();
            float multiplier = modPlayer.IsFlaring ? 2.0f : 1.0f;
            
            // Apply buffs with multiplier
            player.GetDamage(DamageClass.Melee) += 0.15f * multiplier;
            player.GetAttackSpeed(DamageClass.Melee) += 0.10f * multiplier;
            player.moveSpeed += 0.8f * multiplier;
            player.maxRunSpeed *= 1.0f + (0.1f * multiplier);
            player.jumpSpeedBoost += 0.6f * multiplier;
            player.statDefense += (int)(8 * multiplier);          
            player.lifeRegen += (int)(4 * multiplier);
            player.endurance += 0.40f * (multiplier * 0.75f); // Scale endurance less aggressively
            
            // Add visual effects when flaring
            if (modPlayer.IsFlaring && Main.rand.NextBool(6))
            {
                // Create dust effect around player to show enhanced pewter burning
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1.5f, 1.5f);
                Dust.NewDustPerfect(
                    player.Center + Main.rand.NextVector2Circular(16f, 16f), 
                    DustID.Silver, 
                    dustVel, 
                    150, 
                    default, 
                    0.8f
                );
            }
        }
    }
}