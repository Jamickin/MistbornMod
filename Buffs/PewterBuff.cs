using Terraria;
using Terraria.ModLoader;
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
            player.GetDamage(DamageClass.Melee) += 0.15f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.10f;
            player.moveSpeed += 0.8f;
            player.maxRunSpeed *= 1.1f;
            player.jumpSpeedBoost += 0.6f;
            player.statDefense += 8;          
            player.lifeRegen += 4;
            player.endurance += 0.40f;
        }
    }
}
