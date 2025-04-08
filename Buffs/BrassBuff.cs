using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
namespace MistbornMod.Buffs
{
    public class BrassBuff : MetalBuff
    {
        private const float SootheRange = 500f;
        private const int DebuffDuration = 120; 

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Metal = MetalType.Brass; 
        }
        public override void Update(Player player, ref int buffIndex)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (npc.active && !npc.friendly && !npc.boss && npc.CanBeChasedBy())
                {
                    float distanceSq = Vector2.DistanceSquared(player.Center, npc.Center);
                    if (distanceSq < SootheRange * SootheRange)
                    {
                        npc.AddBuff(BuffID.Slow, DebuffDuration);
                        npc.target = Main.maxPlayers;                       
                    }
                }
            }
        }
    }
}
