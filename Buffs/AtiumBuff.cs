using Terraria;
using Terraria.ID;
namespace MistbornMod.Buffs
{
    public class AtiumBuff : MetalBuff
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Metal = MetalType.Atium;
            Main.buffNoTimeDisplay[Type] = true; // Hide default timer
            Main.pvpBuff[Type] = true; 
            Main.debuff[Type] = false; 
        }

        public override void ApplyBuffEffect(Player player, bool isFlaring)
        {
            player.immune = true; 
            player.immuneTime = 3; 
            if (Main.rand.NextBool(4)) {
                 Dust.NewDust(player.position, player.width, player.height, DustID.Shadowflame, 0f, 0f, 150, default, 0.6f);
            }
        }
    }
}