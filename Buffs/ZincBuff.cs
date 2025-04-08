using Terraria;
using Terraria.ID; 
using Microsoft.Xna.Framework; 
namespace MistbornMod.Buffs 
{
    public class ZincBuff : MetalBuff 
    {
        private const float RiotRange = 350f; 
        private const int DebuffDuration = 180; 

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
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.boss && npc.CanBeChasedBy()) 
                {
                    float distanceSq = Vector2.DistanceSquared(player.Center, npc.Center);
                    if (distanceSq < RiotRange * RiotRange)
                    {
                        npc.target = player.whoAmI; 
                        npc.AddBuff(BuffID.Ichor, DebuffDuration);
                        if (Main.rand.NextBool(6)) 
                        {
                             Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood, npc.velocity.X * 0.3f, npc.velocity.Y * 0.3f, 50, default, 0.8f); // Reddish dust
                        }
                    }
                }
            }
        }
    }
}
