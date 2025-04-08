using Terraria;
using Terraria.ModLoader;
namespace MistbornMod.Buffs
{
    public class TinBuff : MetalBuff
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Metal = MetalType.Tin;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.nightVision = true;
            player.detectCreature = true;
            player.dangerSense = true;
            player.GetCritChance(DamageClass.Generic) += 15; 
        }
    }
}
