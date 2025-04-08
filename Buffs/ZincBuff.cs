using Terraria;
using Terraria.ID; 
using Microsoft.Xna.Framework; 
namespace MistbornMod.Buffs 
{
    public class ZincBuff : MetalBuff 
    {
        private const float RiotRange = 350f; // Base riot range
        private const int BaseDebuffDuration = 180; // Base debuff duration (3 seconds)

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults(); 
        }
        
        public override void Load() {
             Metal = MetalType.Zinc; 
        }

        public override void Update(Player player, ref int buffIndex)
        {
            base.Update(player, ref buffIndex);
            
            // Get the MistbornPlayer instance to check flaring status
            MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>();
            float multiplier = modPlayer.IsFlaring ? 2.0f : 1.0f;
            
            // Calculate dynamic values based on flaring
            float currentRiotRange = RiotRange * multiplier;
            int currentDebuffDuration = (int)(BaseDebuffDuration * multiplier);
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.boss && npc.CanBeChasedBy()) 
                {
                    float distanceSq = Vector2.DistanceSquared(player.Center, npc.Center);
                    if (distanceSq < currentRiotRange * currentRiotRange)
                    {
                        npc.target = player.whoAmI; 
                        
                        // Apply Ichor debuff (reduced defense)
                        npc.AddBuff(BuffID.Ichor, currentDebuffDuration);
                        
                        // When flaring, also apply additional debuffs to represent more intense rioting
                        if (modPlayer.IsFlaring)
                        {
                            // Apply On Fire! debuff to represent rage
                            npc.AddBuff(BuffID.OnFire, currentDebuffDuration);
                            
                            // Apply Confusion to represent mental instability from intense rioting
                            npc.AddBuff(BuffID.Confused, currentDebuffDuration / 2);
                            
                            // Make enemies more aggressive when flaring
                            npc.takenDamageMultiplier += 0.2f; // Take 20% more damage due to reckless behavior
                        }
                        
                        // More intense dust effects when flaring
                        if (Main.rand.NextBool(modPlayer.IsFlaring ? 3 : 6)) 
                        {
                            int dustType = modPlayer.IsFlaring ? DustID.Blood : DustID.FireworksRGB;
                            float scale = modPlayer.IsFlaring ? 1.2f : 0.8f;
                            
                            Dust.NewDust(
                                npc.position, 
                                npc.width, 
                                npc.height, 
                                dustType, 
                                npc.velocity.X * 0.3f, 
                                npc.velocity.Y * 0.3f, 
                                50, 
                                default, 
                                scale
                            );
                        }
                    }
                }
            }
            
            // Visual effect around player to show zinc burning
            if (modPlayer.IsFlaring && Main.rand.NextBool(8))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(32f, 32f);
                Dust.NewDustPerfect(dustPos, DustID.Torch, Vector2.Zero, 150, Color.Red, 0.7f);
            }
        }
    }
}